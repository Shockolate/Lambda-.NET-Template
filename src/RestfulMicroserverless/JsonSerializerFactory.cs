using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestfulMicroserverless.Contracts;

namespace RestfulMicroseverless
{
    public static class JsonSerializerFactory
    {
        public static JsonSerializerSettings CreateDefaultJsonSerializerSettings() => new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static IPayloadSerializer CreateJsonPayloadSerializer()
        {
            var settings = CreateDefaultJsonSerializerSettings();
            return new JsonPayloadSerializer(JsonSerializer.Create(settings), settings);
        }
    }
}