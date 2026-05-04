using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Models
{
    public class JsonWebKey
    {
        [JsonProperty("alg", NullValueHandling = NullValueHandling.Ignore)]
        public string? Alg { get; set; } = null;

        [JsonProperty("e", NullValueHandling = NullValueHandling.Ignore)]
        public string? E { get; set; } = null;

        [JsonProperty("key_ops")]
        public string[]? Key_ops { get; set; } = null;

        [JsonProperty("kid", NullValueHandling = NullValueHandling.Ignore)]
        public string? Kid { get; set; } = null;

        [JsonProperty("kty", Required = Required.Always)]
        public string Kty { get; set; } = string.Empty;

        [JsonProperty("n", NullValueHandling = NullValueHandling.Ignore)]
        public string? N { get; set; } = null;

        [JsonProperty("use", NullValueHandling = NullValueHandling.Ignore)]
        public string? Use { get; set; } = null;

        [JsonProperty("x5t", NullValueHandling = NullValueHandling.Ignore)]
        public string? X5t { get; set; } = null;

        [JsonProperty("x5c")]
        public string[]? X5c { get; set; } = null;

        // -------------------------
        // Serialization controls
        // -------------------------
        public bool ShouldSerializeKey_ops() => this.Key_ops is { Length: > 0 };

        public bool ShouldSerializeX5c() => this.X5c is { Length: > 0 };
    }
}
