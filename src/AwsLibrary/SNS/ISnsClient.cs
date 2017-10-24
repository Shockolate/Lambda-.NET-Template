using System.Threading.Tasks;
using RestfulMicroserverless.Contracts;

namespace AwsLibrary.SNS
{
    public interface ISnsClient
    {
        Task PublishMessageToTopicAsync(string message, ILogger logger);
    }
}