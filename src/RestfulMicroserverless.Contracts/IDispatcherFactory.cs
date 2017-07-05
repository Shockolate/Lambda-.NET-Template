using System.Collections.Generic;

namespace RestfulMicroserverless.Contracts
{
    public interface IDispatcherFactory
    {
        IDispatcher CreateDispatcher(IEnumerable<IHttpPathHandler> pathHandlers);
    }
}