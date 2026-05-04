using System;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Models
{
    public class JsonWebKey
    {
        [JsonProperty("alg")]
        public string Alg { get; set; } = string.Empty;

        [JsonProperty("e")]
        public string E { get; set; } = string.Empty;

        [JsonProperty("key_ops")]
        public string[] Key_ops { get; set; } = [];

        [JsonProperty("kid")]
        public string Kid { get; set; } = string.Empty;

        [JsonProperty("kty")]
        public string Kty { get; set; } = string.Empty;

        [JsonProperty("n")]
        public string N { get; set; } = string.Empty;

        [JsonProperty("use")]
        public string Use { get; set; } = string.Empty;

        [JsonProperty("x5t")]
        public string X5t { get; set; } = string.Empty;

        [JsonProperty("x5c")]
        public string[] X5c { get; set; } = [];
    }
}
