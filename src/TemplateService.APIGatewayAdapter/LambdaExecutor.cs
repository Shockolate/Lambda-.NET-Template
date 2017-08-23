using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using AwsLibrary;
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
        private readonly IPayloadConverter _payloadConverter;

        public LambdaExecutor() : this(
            new Dispatcher(TemplateServiceComposer.CreatePathHandlers(new HttpPathHandlerFactory(), JsonSerializerFactory.CreateJsonPayloadSerializer())),
            new LambdaLoggerWrapper(), JsonSerializerFactory.CreateJsonPayloadSerializer()) { }


        internal LambdaExecutor(IDispatcher dispatcher, ILogger logger, IPayloadConverter payloadConverter)
        {
            _dispatcher = dispatcher;
            _lambdaLogger = logger;
            _payloadConverter = payloadConverter;
        }

        public async Task<APIGatewayProxyResponse> ApiGatewayProxyInvocation(APIGatewayProxyRequest apiGatewayProxyRequest, ILambdaContext context)
        {
            var targetVerbosity = Verbosity.Silent;
            if (apiGatewayProxyRequest.StageVariables.ContainsKey("verbosity"))
                Enum.TryParse(apiGatewayProxyRequest.StageVariables["verbosity"], out targetVerbosity);
            _lambdaLogger.Verbosity = targetVerbosity;
            _lambdaLogger.LogDebug("Invoked!");

            apiGatewayProxyRequest.LogEventDebug(_lambdaLogger);
            try
            {
                var restRequest = CreateRestRequest(apiGatewayProxyRequest);
                var restResponse = await _dispatcher.DispatchAsync(restRequest, _lambdaLogger);
                return CreateApiGatewayProxyResponse(restResponse);
            }
            catch (ArgumentException e)
            {
                return new APIGatewayProxyResponse {Body = _payloadConverter.ConvertToPayload(new {errorMessage = e.Message}), StatusCode = 405};
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
                Body = _payloadConverter.ConvertToPayload(restResponse.Body),
                Headers = restResponse.Headers,
                StatusCode = restResponse.StatusCode
            };
        }
    }
}