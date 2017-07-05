using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    internal class HttpPathHandler : IHttpPathHandler
    {
        private readonly Route _route;
        private readonly IDictionary<HttpVerb, Func<RestRequest, Task<RestResponse>>> _verbHandlers;

        internal HttpPathHandler(Route route, IDictionary<HttpVerb, Func<RestRequest, Task<RestResponse>>> verbHandlers)
        {
            _route = route;
            _verbHandlers = verbHandlers;
        }

        public ILogger Logger { get; protected set; }

        public async Task<RestResponse> HandleAsync(RestRequest request, ILogger logger)
        {
            Logger = logger;
            Logger.LogInfo($"Handling {request.Method} for {_route}");
            request.PathParameters = _route.BuildPathParameters(request.InvokedPath);
            return await _verbHandlers[request.Method].Invoke(request);
        }

        public bool CanHandle(RestRequest request)
        {
            return _route.Matches(request.InvokedPath) && _verbHandlers.ContainsKey(request.Method);
        }
    }
}