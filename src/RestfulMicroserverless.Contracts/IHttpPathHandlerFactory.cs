using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestfulMicroserverless.Contracts
{
    public interface IHttpPathHandlerFactory
    {
        IHttpPathHandler CreateHttpPathHandler(string path, IDictionary<HttpVerb, Func<RestRequest, ILogger, Task<RestResponse>>> verbHandlers);
    }
}