using CDR.Register.Domain.Models;
using Newtonsoft.Json;

namespace CDR.Register.SSA.API.Business
{
    public static class SerializationExtensions
    {
        public static readonly JsonSerializerSettings DefaultSerializationConfiguration = new CdrJsonSerializerSettings();

        public static string ToJson(this object value)
            => ToJson(value, DefaultSerializationConfiguration);

        public static string ToJson(this object value, JsonSerializerSettings options)
            => JsonConvert.SerializeObject(value, options);
    }
}
