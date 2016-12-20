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

STDOUT.sync = true
STDERR.sync = true

ROOT = File.dirname(__FILE__)
SRC_DIR = File.join(ROOT, 'src')
IMPLEMENTATION_DIR = File.join(SRC_DIR, 'Implementation')
UNIT_TESTS_DIR = File.join(SRC_DIR, 'UnitTests')
PACKAGE_DIR = File.join(ROOT, 'package')
CONFIG_DIR = File.join(ROOT, 'config')

CLEAN.include(PACKAGE_DIR)
CLEAN.include(File.join(ROOT, 'reports'))
CLEAN.include(File.join(SRC_DIR, '**/bin'), File.join(SRC_DIR, '**/obj'), 'output')
CLEAN.include(File.join(ROOT, '**/TestResult.xml'))

# Developer tasks
desc 'Lints, unit tests, and builds the package directory.'
task :build => [:parse_config, :retrieve, :dotnet_build, :unit_test]

desc 'Deploys a deployment package to Lambda in the specified environment/stage.'
task :deploy_to_lambda, [:environment] => [:parse_config] do |t, args|
  #validate params
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?

  # publish to S3
  puts 'Uploading deployment package to S3...'
  t1 = Time.now
  s3_version_id = publish_lambda_package_to_s3()
  t2 = Time.now
  puts "Uploaded deployment package to S3. #{t2 - t1}"

  #deploy functions
  puts 'Deploying Lambda Functions...'
  t1 = Time.now
  CONFIGURATION["functions"].each do |f|
    func_version = deploy_lambda(s3_version_id, f["name"], f["handler"], f["description"], f["timeout"], f["memory_size"])
    promote_lambda(f["name"], func_version, env)
  end
  t2 = Time.now
  puts "Deployed Lambda Functions. #{t2 - t1}"
end

desc 'Deploys OpenAPI (Swagger) Specification to the specified environment/stage with specified
  verbosity. Defaults to DEBUG verbosity if none specified. Uploads swagger file for swagger
  resource if environment == production.'
task :deploy_to_apigateway, [:environment, :verbosity] => [:parse_config] do |t, args|
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?
  verbos = args[:verbosity]
  verbos = 'DEBUG' if verbos.nil?
  stage_variables = { 'verbosity' => verbos }
  setup_apigateway(env, CONFIGURATION["api_name"], stage_variables)
end

task :deploy_environment, [:environment, :verbosity] => [:package, :deploy_to_lambda, :deploy_to_apigateway]

# Jenkins Targets
# TODO: Implement dependencies.
task :merge_job, [:environment, :verbosity] => [:clean, :parse_config, :retrieve, :lint, :dotnet_build, :unit_test, :package, :deploy_environment]
task :pull_request_job => [:clean, :parse_config, :retrieve, :lint, :dotnet_build, :unit_test, :package, :integration_test, :e2e_test ]

# Workflow tasks
desc 'Retrieves external dependencies. Calls "dotnet restore"'
task :retrieve do
  cmd = "dotnet restore"
  raise 'Node Modules not installed.' if !system(cmd)
end

desc 'Runs Unit tests located in the src/UnitTests project.'
task :unit_test do
  cmd = "dotnet test #{UNIT_TESTS_DIR} --no-build"
  raise 'Error running unit tests.' if !system(cmd)
end

task :dotnet_build do
  cmd = "dotnet build #{IMPLEMENTATION_DIR} #{UNIT_TESTS_DIR}"
  raise 'Error running building.' if !system(cmd)
end

task :lint do
  begin
    api = Swagger.load(File.join(CONFIG_DIR, 'ClientSwagger.yaml'))
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

desc 'Promotes an existing Lambda Function with Version to a given environment/stage.'
task :promote, [:environment, :function_name, :function_version] do
  # validate input parameters
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?
  function_name = args[:function_name]
  raise 'Parameter function needs to be set' if function_name.nil?
  function_version = args[:function_version]
  raise 'Parameter version needs to be set' if function_version.nil?

  # promote a specific lambda function version
  promote_lambda(function_name, function_version, env)
end

desc 'tears down an environment - Removes Lambda Aliases and Deletes API Gateway Stage.'
task :teardown_environment, [:environment] => [:parse_config] do |t, args|
  # validate input parameters
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?

  teardown_apigateway_stage(env)

  teardown_lambda_aliases(env)
end

desc 'Tears down API Gateway Stage.'
task :teardown_apigateway_stage, [:environment] => [:parse_config] do |t, args|
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?
  teardown_apigateway_stage(env)
end

desc 'Tears down Lambda Environment.'
task :teardown_lambda_aliases, [:environment] => [:parse_config] do |t, args|
  env = args[:environment]
  raise 'Parameter environment needs to be set' if env.nil?
  teardown_lambda_aliases(env)
end

task :parse_config do
  puts 'Parsing config...'
  CONFIGURATION = YAML::load_file(File.join(CONFIG_DIR, 'config.yaml'))
  swaggerFile = YAML::load_file(File.join(CONFIG_DIR, 'ClientSwagger.yaml'))
  CONFIGURATION['api_name'] = swaggerFile['info']['title']
  puts 'parsed. '
end

def publish_lambda_package_to_s3()
  lm = LambdaWrap::LambdaManager.new()
  return lm.publish_lambda_to_s3(File.join(PACKAGE_DIR, CONFIGURATION["deployment_package_name"]), CONFIGURATION["s3"]["lambda"]["bucket"], CONFIGURATION["s3"]["lambda"]["key"])
end

def deploy_lambda(s3_version_id, function_name, handler_name, lambda_description, timeout, memory_size)
  lambdaMgr = LambdaWrap::LambdaManager.new()
  func_version = lambdaMgr.deploy_lambda(CONFIGURATION["s3"]["lambda"]["bucket"], CONFIGURATION["s3"]["lambda"]["key"],
    s3_version_id, function_name, handler_name, CONFIGURATION["lambda_role_arn"], lambda_description,
    CONFIGURATION["subnet_ids"], CONFIGURATION["security_groups"], "dotnetcore1.0", timeout, memory_size)
   puts "Deployed #{function_name} to function version #{func_version}."
  return func_version

end

def promote_lambda(function_name, func_version, env)
  lambdaMgr = LambdaWrap::LambdaManager.new()
  lambdaMgr.create_alias(function_name, func_version, env)
end

def setup_apigateway(env, api_name, stage_variables)
  # delegate to api gateway manager
  puts "Setting up #{api_name} on API Gateway and deploying to Environment: #{env}...."
  t1 = Time.now
  swagger_file = File.join(CONFIG_DIR, 'APIGatewaySwagger.yaml')
  mgr = LambdaWrap::ApiGatewayManager.new()
  uri = mgr.setup_apigateway_by_swagger_file(api_name, File.join(CONFIG_DIR, 'APIGatewaySwagger.yaml'), env, stage_variables)
  t2 = Time.now
  puts "API gateway with api name set to #{api_name} and environment #{env} is available at #{uri}"
  puts "Took #{t2 - t1} seconds."

  # Upload API spec for Swagger UI
  if env == 'production'
    upload_swagger_file()
  end
  return uri
end

def upload_swagger_file()
#  cleaned_swagger = clean_swagger(YAML::load_file(File.join(CONFIG_DIR, 'swagger.yaml')))
  puts "uploading Swagger File..."
  swaggerFile = File.open(File.join(CONFIG_DIR, 'ClientSwagger.yaml'))
  swaggerString = swaggerFile.read
  s3 = Aws::S3::Client.new()
  s3.put_object(acl: 'public-read', body: swaggerString, bucket: CONFIGURATION["s3"]["swagger"]["bucket"],
    key: CONFIGURATION["s3"]["swagger"]["key"])
  swaggerFile.close
  puts "Swagger File uploaded."
end

def package()
  puts 'Publishing & zipping binaries...'
  t1 = Time.now
  Dir.chdir(IMPLEMENTATION_DIR)
  cmd = "dotnet lambda package -pl #{IMPLEMENTATION_DIR} -c Release -f netcoreapp1.0 -o #{File.join(PACKAGE_DIR, CONFIGURATION["deployment_package_name"])}"
  raise 'Error creating deployment package.' if !system(cmd)
  Dir.chdir(ROOT)
  t2 = Time.now
  puts 'Zipped binaries. ' + (t2-t1).to_s
=begin puts 'Downloading secrets zip....'
  t1 = Time.now
  s3 = Aws::S3::Client.new()
  s3.get_object(
    response_target: PACKAGE_DIR + '/' + CONFIGURATION["s3"]["secrets"]["key"],
    bucket: CONFIGURATION["s3"]["secrets"]["bucket"],
    key: CONFIGURATION["s3"]["secrets"]["key"],
  )
  t2 = Time.now
  puts 'Secrets downloaded. ' + (t2-t1).to_s

  secrets_entries = Array.new
  puts 'Extracting Secrets...'

  t1 = Time.now
  Zip::File.open(PACKAGE_DIR + '/' + CONFIGURATION["s3"]["secrets"]["key"]) do |secrets_zip_file|
    secrets_zip_file.each do |entry|
      secrets_entries.push(entry.name)
      entry.extract(PACKAGE_DIR + '/' + entry.name)
    end
  end
  t2 = Time.now
  puts 'Secrets Extracted. ' + (t2 - t1).to_s

  puts 'Adding secrets to package...'
  t1 = Time.now
  Zip::File.open(File.join(PACKAGE_DIR, CONFIGURATION["deployment_package_name"]), Zip::File::CREATE) do |zipfile|
    secrets_entries.each do |entry|
      zipfile.add(entry, PACKAGE_DIR + '/' + entry)
    end
  end
  t2 = Time.now
  puts 'Added secrets to package. ' + (t2 - t1).to_s
  #TODO Cleanup secrets?
  puts "\n"
=end
  puts 'Successfully created the deployment package!'
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

def teardown_apigateway_stage(stage)
  puts "Deleting Stage: #{stage} from API: #{CONFIGURATION['api_name']}...."
  t1 = Time.now
  LambdaWrap::ApiGatewayManager.new.shutdown_apigateway(CONFIGURATION["api_name"], stage)
  t2 = Time.now
  puts "Deleted. #{t2 - t1}"
end

def teardown_lambda_aliases(aliasValue)
  puts "Deleting Alias: #{aliasValue} from the lambdas."
  t1 = Time.now
  lm = LambdaWrap::LambdaManager.new()
  CONFIGURATION["functions"].each do |f|
    lm.remove_alias(f["name"], aliasValue)
  end
  t2 = Time.now
  puts "Deleted. #{t2 - t1}"
end
