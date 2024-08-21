using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace CDR.Register.Domain.Models
{
    public class CdrJsonSerializerSettings : JsonSerializerSettings
    {
        public CdrJsonSerializerSettings() : base()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver();
            DefaultValueHandling = DefaultValueHandling.Include;
            NullValueHandling = NullValueHandling.Ignore;
            Formatting = Formatting.Indented;
            Converters = new List<JsonConverter>() { new StringEnumConverter() };
        }
    }
}
