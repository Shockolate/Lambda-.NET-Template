using System.IO;

namespace RestfulMicroserverless.Contracts
{
    public interface IPayloadSerializer
    {
        string SerializePayload(object objectToConvert);
        void SerializePayload(object objectToConvert, Stream targetStream);
        T DeserializePayload<T>(string payload);
        T DeserializePayload<T>(Stream stream);
    }
}