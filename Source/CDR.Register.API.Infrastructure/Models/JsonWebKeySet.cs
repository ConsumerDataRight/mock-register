using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Models
{
    public class JsonWebKeySet
    {
        [JsonProperty("keys")]
        public JsonWebKey[] Keys { get; set; } = [];
    }
}
