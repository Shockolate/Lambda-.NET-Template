using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    public class Dispatcher : IDispatcher
    {
        private readonly RestResponseFactory _restResponseFactory;
        protected readonly IEnumerable<IHttpPathHandler> Handlers;

        public Dispatcher(IEnumerable<IHttpPathHandler> handlers)
        {
            _restResponseFactory = new RestResponseFactory(JsonSerializerFactory.CreateJsonPayloadSerializer());
            Handlers = handlers;
        }

        public async Task<RestResponse> DispatchAsync(RestRequest request, ILogger logger)
        {
            try
            {
                return await Handlers.Single(handler => handler.CanHandle(request)).HandleAsync(request, logger);
            }

            // Thrown when no valid handlers, or more than one valid handler.
            catch (InvalidOperationException)
            {
                logger.LogError(() => $"No Singular Matching Handler found for {request.Method}, {request.InvokedPath}");
                return _restResponseFactory.CreateMethodNotAllowedWithCorsRestResponse(request.Method, request.InvokedPath);
            }
            catch (Exception e)
            {
                logger.LogError(() => e.Message);
                var response = _restResponseFactory.CreateErrorMessageRestResponse($"InternalServerError: {e.Message}");
                // 500 - Internal Server Error
                response.StatusCode = 500;
                return response;
            }
        }
    }
}