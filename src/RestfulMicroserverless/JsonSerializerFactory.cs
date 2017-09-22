using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    public static class JsonSerializerFactory
    {
        public static JsonSerializerSettings CreateDefaultJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
        }

        public static IPayloadSerializer CreateJsonPayloadSerializer()
        {
            var settings = CreateDefaultJsonSerializerSettings();
            return new JsonPayloadSerializer(JsonSerializer.Create(settings), settings);
        }
    }
}