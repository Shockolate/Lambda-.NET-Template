using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulMicroserverless.Contracts;

namespace TemplateService.HttpPathHandler
{
    public class TemplatePathHandler
    {
        private readonly RestResponseFactory _restResponseFactory;

        public TemplatePathHandler(RestResponseFactory responseFactory)
        {
            _restResponseFactory = responseFactory;
            VerbHandlers.Add(HttpVerb.Get, GetAsync);
        }

        internal IDictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> VerbHandlers { get; } =
            new Dictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>>();

        public async Task<RestResponse> GetAsync(RestRequest request, ILogger logger)
        {
            return await Task.FromResult(_restResponseFactory.CreateCorsRestResponse(200));
        }
    }
}