using System.Collections.Generic;
using RestfulMicroserverless.Contracts;
using TemplateService.HttpPathHandler;

namespace TemplateService
{
    public static class TemplateServiceComposer
    {
        public static IEnumerable<IHttpPathHandler> CreatePathHandlers(IHttpPathHandlerFactory pathHandlerFactory, IPayloadSerializer payloadConverter)
        {
            var restResponseFactory = new RestResponseFactory(payloadConverter);
            var templatePathHandler = new TemplatePathHandler(restResponseFactory);
            return new List<IHttpPathHandler> {pathHandlerFactory.CreateHttpPathHandler("template/path", templatePathHandler.VerbHandlers)};
        }
    }
}
