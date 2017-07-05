using System.Threading.Tasks;

namespace RestfulMicroserverless.Contracts
{
    public interface IDispatcher
    {
        Task<RestResponse> DispatchAsync(RestRequest request, ILogger logger);
    }
}
