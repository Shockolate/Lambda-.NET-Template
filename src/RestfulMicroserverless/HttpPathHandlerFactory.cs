using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    public class HttpPathHandlerFactory : IHttpPathHandlerFactory
    {
        public IHttpPathHandler CreateHttpPathHandler(string path, IDictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> verbHandlers)
        {
            return new HttpPathHandler(new Route(path), verbHandlers);
        }
    }
}