using System.IO;
using Newtonsoft.Json;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    internal class JsonPayloadSerializer : IPayloadSerializer
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonPayloadSerializer(JsonSerializer jsonSerializer, JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializer = jsonSerializer;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public string SerializePayload(object objectToConvert) => JsonConvert.SerializeObject(objectToConvert, _jsonSerializerSettings);

        public void SerializePayload(object objectToConvert, Stream targetStream)
        {
            using (var streamWriter = new StreamWriter(targetStream))
            using (var textWriter = new JsonTextWriter(streamWriter))
            {
                _jsonSerializer.Serialize(textWriter, objectToConvert);
            }
        }

        public T DeserializePayload<T>(string payload)
        {
            using (var stringReader = new StringReader(payload))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return _jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public T DeserializePayload<T>(Stream payloadStream)
        {
            using (var streamReader = new StreamReader(payloadStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return _jsonSerializer.Deserialize<T>(jsonReader);
            }
        }
    }
}