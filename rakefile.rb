require 'aws-sdk'
Aws.use_bundled_cert!
require 'rake'
require 'rake/clean'
require 'fileutils'
require 'json'
require 'lambda_wrap'
require 'yaml'
require 'zip'
require 'forwardable'
require 'swagger'
require 'active_support/core_ext/hash'
require 'mail'

STDOUT.sync = true
STDERR.sync = true

ROOT = File.dirname(__FILE__)
SRC_DIR = File.join(ROOT, 'src')
PACKAGE_DIR = File.join(ROOT, 'package')
CONFIG_DIR = File.join(ROOT, 'config')
REPORTS_DIR = File.join(ROOT, 'reports')

CLEAN.include(PACKAGE_DIR)
CLEAN.include(File.join(ROOT, 'reports'))
CLEAN.include(File.join(SRC_DIR, '**/bin'), File.join(SRC_DIR, '**/obj'), 'output')
CLEAN.include(REPORTS_DIR)

# Developer tasks
desc 'Compiles the source code to binaries.'
task :build => [:clean, :parse_config, :retrieve, :dotnet_build]
desc 'Builds and runs unit tests.'
task :test => [:build, :unit_test]

# Commit Targets
task :merge_job  => [:clean, :parse_config, :retrieve, :lint, :dotnet_build, :unit_test, :package, :deploy_production, :release_notification]
task :pull_request_job => [:clean, :parse_config, :retrieve, :lint, :dotnet_build, :unit_test, :package, :deploy_test]

task :deploy_production => [:parse_config, :build, :package] do
  deploy(:production)
  # TODO
  # upload_swagger_file
end

task :deploy_test => [:parse_config, :build, :package] do
  deploy(:test)
end

task :deploy_environment, [:environment, :verbosity] => [:build] do |t, args|
  raise 'Parameter environment needs to be set' if args[:environment].nil?
  raise 'Parameter verbosity needs to be set' if args[:verbosity].nil?
  API.deploy(LambdaWrap::Environment.new(args[:environment], { 'verbosity' => args[:verbosity] }))
end

desc 'Don\'t.'
task :teardown_production => [:parse_config] do
  teardown(:production)
end

desc 'If you want.'
task :teardown_test => [:parse_config] do
  teardown(:test)
end

desc 'tears down an environment - Removes Lambda Aliases and Deletes API Gateway Stage.'
task :teardown_environment, [:environment] => [:parse_config] do |t, args|
  # validate input parameters
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?
  API.teardown(LambdaWrap::Environment.new(name: args[:environment]))
end

# Workflow tasks
desc 'Retrieves external dependencies. Calls "dotnet restore"'
task :retrieve do
  Dir["#{SRC_DIR}/**/*.csproj"].each do |csproj_path|
    raise "Dependency installation failed for #{csproj_path}" unless system("dotnet restore #{csproj_path} --verbosity normal")
  end
end

desc 'Runs Unit tests located in the src/UnitTests project.'
task :unit_test do
  Dir["#{SRC_DIR}/**/*.UnitTests/*.csproj"].each do |csproj_path|
    raise "Error running unit tests: #{csproj_path}" unless system("dotnet test #{csproj_path} --no-build --logger:trx;LogFileName=#{File.join(REPORTS_DIR, 'testresults.trx')}")
  end
end

task :dotnet_build do
  Dir["#{SRC_DIR}/**/*.csproj"].each do |csproj_path|
    raise "Error building: #{csproj_path}" unless system("dotnet build #{csproj_path} --framework netcoreapp1.1")
  end
end

task :lint do
  begin
    Swagger.load(File.join(CONFIG_DIR, 'ClientSwagger.yaml'))
    puts 'Valid swagger file.'
  rescue Exception => e
    puts e.message
    raise 'Invalid Swagger File!'
  end
end

desc 'Creates a package for deployment.'
task :package => [:parse_config, :retrieve, :dotnet_build] do
  package()
end

task :parse_config do
  puts 'Parsing config...'
  CONFIGURATION = YAML::load_file(File.join(CONFIG_DIR, 'config.yaml')).deep_symbolize_keys
  API = LambdaWrap::API.new()

  ENVIRONMENTS = {}

  CONFIGURATION[:environments].each do |e|
    ENVIRONMENTS[e[:name].to_sym] = LambdaWrap::Environment.new(e[:name], e[:variables], e[:description])
  end

  API.add_lambda(
    CONFIGURATION[:lambdas].map do |lambda_config|
      lambda_config[:path_to_zip_file] = File.join(PACKAGE_DIR, 'deployment_package.zip')
      LambdaWrap::Lambda.new(lambda_config)
    end
  )

  API.add_api_gateway(
    LambdaWrap::ApiGateway.new(path_to_swagger_file: File.join(CONFIG_DIR, 'APIGatewaySwagger.yaml'))
  )

  Mail.defaults do
    delivery_method :smtp, address: 'relay.vistaprint.net', port: 25
  end

  puts 'parsed. '
end

task :release_notification => [:parse_config] do
  release_notification(CONFIGURATION[:application_name], ENV.fetch('BUILD_NUMBER', 'BuildNumberNotSet'))
end


def upload_swagger_file()
  cleaned_swagger = clean_swagger(YAML::load_file(File.join(CONFIG_DIR, 'ClientSwagger.yaml')))
  puts "uploading Swagger File..."
  s3 = Aws::S3::Client.new()
  s3.put_object(acl: 'public-read', body: cleaned_swagger, bucket: CONFIGURATION[:s3][:swagger][:bucket],
    key: CONFIGURATION[:s3][:swagger][:key])
  puts "Swagger File uploaded."
end

def package()
  puts 'Creating the deployment package...'
  t1 = Time.now

  FileUtils.mkdir(PACKAGE_DIR, verbose: true)
  cmd = "dotnet publish #{File.join(SRC_DIR, CONFIGURATION[:composition_root_project])} --configuration Release --framework netcoreapp1.1 --output #{PACKAGE_DIR} --verbosity normal"
  raise 'Error creating deployment package.' if !system(cmd)


  zip_package

  if CONFIGURATION[:s3] && CONFIGURATION[:s3][:secrets] && CONFIGURATION[:s3][:secrets][:bucket] && CONFIGURATION[:s3][:secrets][:key]
    download_secrets
    secrets = extract_secrets
    add_secrets_to_package(secrets)
    cleanup_secrets(secrets)
  end

  t2 = Time.now
  puts
  puts "Successfully created the deployment package! #{t2 - t1}"
end

def zip_package
  Zip::File.open(File.join(PACKAGE_DIR, 'deployment_package.zip'), Zip::File::CREATE) do |io|
    write_entries(filter_entries(PACKAGE_DIR), '', io, PACKAGE_DIR)
  end
end

def filter_entries(directory)
  Dir.entries(directory) - %w[. ..]
end

def write_entries(entries, path, io, input_directory)
  entries.each do |e|
    zip_file_path = path == '' ? e : File.join(path, e)
    disk_file_path = File.join(input_directory, zip_file_path)
    puts "Deflating #{disk_file_path}"

    if File.directory? disk_file_path
      recursively_deflate_directory(disk_file_path, io, zip_file_path, input_directory)
    else
      put_into_archive(disk_file_path, io, zip_file_path)
    end
  end
end

def recursively_deflate_directory(disk_file_path, io, zip_file_path, input_directory)
  io.mkdir zip_file_path
  write_entries(filter_entries(disk_file_path), zip_file_path, io, input_directory)
end

def put_into_archive(disk_file_path, io, zip_file_path)
  io.add(zip_file_path, disk_file_path)
end

def download_secrets
  puts 'Downloading secrets zip...'
  s3 = Aws::S3::Client.new()
  s3.get_object(
    response_target: PACKAGE_DIR + '/' + CONFIGURATION[:s3][:secrets][:key],
    bucket: CONFIGURATION[:s3][:secrets][:bucket],
    key: CONFIGURATION[:s3][:secrets][:key]
  )
  puts 'Secrets downloaded. '
end

def extract_secrets
  secrets_entries = Array.new
  puts 'Extracting Secrets...'
  Zip::File.open(PACKAGE_DIR + '/' + CONFIGURATION[:s3][:secrets][:key]) do |secrets_zip_file|
    secrets_zip_file.each do |entry|
      secrets_entries.push(entry.name)
      entry.extract(File.join(PACKAGE_DIR, entry.name))
    end
  end
  puts 'Secrets Extracted. '
  secrets_entries
end

def add_secrets_to_package(secrets)
  puts 'Adding secrets to package...'
  Zip::File.open(File.join(PACKAGE_DIR, 'deployment_package.zip'), Zip::File::CREATE) do |zipfile|
    secrets.each do |entry|
      zipfile.add(entry, File.join(PACKAGE_DIR, entry))
    end
  end
  puts 'Added secrets to package. '
end

def cleanup_secrets(secrets)
  puts 'Cleaning up secrets...'
  secrets << CONFIGURATION[:s3][:secrets][:key]
  FileUtils.rm(secrets.map { |secret| File.join(PACKAGE_DIR, secret) }, verbose: true)
  puts 'Cleaned up secrets.'
end

def clean_swagger(swagger_yaml)
  puts "cleaning Swagger File..."
  swagger_yaml["paths"].each do |pathKey, pathValue|
    swagger_yaml["paths"][pathKey].each do |methodKey, methodValue|
      swagger_yaml["paths"][pathKey][methodKey] = methodValue.reject{|key, value| key == "x-amazon-apigateway-integration"}
    end
  end
  swagger_yaml["paths"] = swagger_yaml["paths"].reject{|key, value| key == "/swagger"}
  puts "cleaned."
  return YAML::dump(swagger_yaml).sub(/^(---\n)/, "")
end

def deploy(environment_symbol)
  raise ArgumentError 'Must pass an environment symbol!' unless environment_symbol.is_a?(Symbol)
  API.deploy(ENVIRONMENTS[environment_symbol])
end

def teardown(environment_symbol)
  raise ArgumentError 'Must pass an environment symbol!' unless environment_symbol.is_a?(Symbol)
  API.deploy(ENVIRONMENTS[environment_symbol])
end

def release_notification(application_name, build_number)
  unless File.file?(File.join(ROOT, 'ReleaseNotes.txt'))
    puts 'No Release Notes. Skipping email.'
    return
  end

  release_notes = File.read(File.join(ROOT, 'ReleaseNotes.txt'))

  puts 'Release Notes:'
  puts
  puts release_notes
  puts

  release_notes.gsub!(/\r\n|\n/, '<br />')

  s3 = Aws::S3::Client.new()
  notification_body = s3.get_object(
    bucket: CONFIGURATION[:s3][:notification_template][:bucket],
    key: CONFIGURATION[:s3][:notification_template][:key]
  ).body.read
  notification_body.sub!('$APPLICATION_NAME', application_name)
  notification_body.sub!('$BUILD_NUMBER', build_number)
  notification_body.sub!('$RELEASE_NOTES', release_notes)

  puts 'Sending Email Notification...'
  t1 = Time.now
  notification = Mail.new do
    from    'MSWProductionShopfloor@vistaprint.com'
    to      'MSWProductionShopfloor@vistaprint.com'
    subject "New Release to #{application_name} - A Shopfloor Service"
    body    notification_body
  end

  notification['Content-Type'] = 'text/html'

  notification.deliver!
  t2 = Time.now
  puts "Sent Email Notification! #{t2 - t1}"

  delete_release_notes
end

def delete_release_notes()
  FileUtils.rm(File.join(ROOT, 'ReleaseNotes.txt'))
  raise 'Error committing ReleaseNotes deletion!' unless system('git add ReleaseNotes.txt --no-ignore-removal && git commit --author="Automatic Jenkins <mswproductionshopfloor@vistaprint.com" -m "Automatic deletion of ReleaseNotes.txt"')
end
