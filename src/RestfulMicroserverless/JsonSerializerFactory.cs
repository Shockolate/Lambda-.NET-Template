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

        public static IPayloadConverter CreateJsonPayloadSerializer()
        {
            return new JsonPayloadConverter(CreateDefaultJsonSerializerSettings());
        }
    }
}
