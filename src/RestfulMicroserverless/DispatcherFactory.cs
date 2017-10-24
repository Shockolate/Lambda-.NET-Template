using System.Collections.Generic;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    internal class DispatcherFactory : IDispatcherFactory
    {
        public IDispatcher CreateDispatcher(IEnumerable<IHttpPathHandler> pathHandlers) => new Dispatcher(pathHandlers);
    }
}