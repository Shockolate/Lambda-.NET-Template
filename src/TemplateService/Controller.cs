using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace TemplateService
{
    public class Controller
    {
        private ILogger _logger;

        public async Task<APIGatewayProxyResponse> Handler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            _logger = new Logger(request.StageVariables.ContainsKey("verbosity") ? request.StageVariables["verbosity"] : "Debug");
            request.LogEventDebug(_logger);
            var returnHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } };
            var response = new APIGatewayProxyResponse { StatusCode = 200, Headers = returnHeaders, Body = "\"Hello World!\"" };
            var timeout = TimeSpan.FromMilliseconds(50);
            await Task.Delay(timeout);
            return response;
        }
    }
}
