using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using AwsLibrary;
using Microsoft.Extensions.Configuration;
using RestfulMicroserverless.Contracts;
using RestfulMicroseverless;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace TemplateService.APIGatewayAdapter
{
    public class LambdaExecutor
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger _lambdaLogger;
        private readonly IPayloadSerializer _payloadConverter;

        public LambdaExecutor() : this(new LambdaLoggerWrapper(), JsonSerializerFactory.CreateJsonPayloadSerializer(),
            new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddInMemoryCollection( /* default config strings */)
                .AddJsonFile("TemplateService.settings", true, true).Build()) { }


        public LambdaExecutor(ILogger logger, IPayloadSerializer payloadSerializer, IConfiguration configuration) : this(
            new Dispatcher(TemplateServiceComposer.CreatePathHandlers(new HttpPathHandlerFactory(), payloadSerializer, configuration)), logger,
            payloadSerializer) { }

        internal LambdaExecutor(IDispatcher dispatcher, ILogger logger, IPayloadSerializer payloadSerializer)
        {
            _dispatcher = dispatcher;
            _lambdaLogger = logger;
            _payloadConverter = payloadSerializer;
        }

        public async Task<APIGatewayProxyResponse> ApiGatewayProxyInvocation(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            var targetVerbosity = Verbosity.Silent;
            if (apiGatewayProxyRequest.StageVariables.ContainsKey("verbosity"))
                Enum.TryParse(apiGatewayProxyRequest.StageVariables["verbosity"], out targetVerbosity);
            _lambdaLogger.Verbosity = targetVerbosity;
            _lambdaLogger.LogDebug(() => "Invoked!");
            _lambdaLogger.LogDebug(() => ApiGatewayProxyHelpers.ProxyRequestToString(apiGatewayProxyRequest));
            try
            {
                var restRequest = CreateRestRequest(apiGatewayProxyRequest);
                var restResponse = await _dispatcher.DispatchAsync(restRequest, _lambdaLogger);
                return CreateApiGatewayProxyResponse(restResponse);
            }
            catch (ArgumentException e)
            {
                return new APIGatewayProxyResponse {Body = _payloadConverter.SerializePayload(new {errorMessage = e.Message}), StatusCode = 405};
            }
        }

        private static RestRequest CreateRestRequest(APIGatewayProxyRequest apiGatewayProxyRequest)
        {
            HttpVerb invokedHttpVerb;
            if (!Enum.TryParse(apiGatewayProxyRequest.HttpMethod, true, out invokedHttpVerb))
                throw new ArgumentException($"HttpMethod: {apiGatewayProxyRequest.HttpMethod} not supported.");
            return new RestRequest
            {
                Body = apiGatewayProxyRequest.Body,
                Method = invokedHttpVerb,
                Headers = apiGatewayProxyRequest.Headers,
                QueryStringParameters = apiGatewayProxyRequest.QueryStringParameters,
                InvokedPath = apiGatewayProxyRequest.Path
            };
        }

        private APIGatewayProxyResponse CreateApiGatewayProxyResponse(RestResponse restResponse)
        {
            return new APIGatewayProxyResponse
            {
                Body = _payloadConverter.SerializePayload(restResponse.Body),
                Headers = restResponse.Headers,
                StatusCode = restResponse.StatusCode
            };
        }
    }
}