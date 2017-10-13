using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RestfulMicroserverless.Contracts;
using TemplateService.HttpPathHandler;

namespace TemplateService
{
    public static class TemplateServiceComposer
    {
        public static IEnumerable<IHttpPathHandler> CreatePathHandlers(IHttpPathHandlerFactory pathHandlerFactory, IPayloadSerializer payloadSerializer,
            IConfiguration configuration)
        {
            // Use Configuration.
            var restResponseFactory = new RestResponseFactory(payloadSerializer);
            var templatePathHandler = new TemplatePathHandler(restResponseFactory);
            return new List<IHttpPathHandler> {pathHandlerFactory.CreateHttpPathHandler("template/path", templatePathHandler.VerbHandlers)};
        }
    }
}