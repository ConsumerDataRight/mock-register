using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CDR.Register.Domain.Models
{
    public class CdrJsonSerializerSettings : JsonSerializerSettings
    {
        public CdrJsonSerializerSettings()
            : base()
        {
            this.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.DefaultValueHandling = DefaultValueHandling.Include;
            this.NullValueHandling = NullValueHandling.Ignore;
            this.Formatting = Formatting.Indented;
            this.Converters = new List<JsonConverter>() { new StringEnumConverter() };
        }
    }
}
