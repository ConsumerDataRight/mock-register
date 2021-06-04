using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CDR.Register.SSA.API.Business
{
    public static class SerializationExtensions
    {
        public static string ToJson(this object value)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            var result = JsonConvert.SerializeObject(value, jsonSerializerSettings);
            return result;
        }
    }
}
