using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestfulMicroserverless.Contracts
{
    public interface IHttpPathHandler
    {
        Task<RestResponse> HandleAsync(RestRequest request, ILogger logger);
        bool CanHandle(RestRequest request);
    }
}
