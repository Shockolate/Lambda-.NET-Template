using Newtonsoft.Json;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    internal class JsonPayloadConverter : IPayloadConverter
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonPayloadConverter(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public string ConvertToPayload(object objectToConvert)
        {
            return JsonConvert.SerializeObject(objectToConvert, _jsonSerializerSettings);
        }

        public T ConvertFromPayload<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }
    }
}