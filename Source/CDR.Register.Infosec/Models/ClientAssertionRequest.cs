using System.Text.Json.Serialization;

namespace CDR.Register.Infosec.Models
{
    public class ClientAssertionRequest
    {
        [JsonPropertyName("grant_type")]
        public string? Grant_type { get; set; }

        [JsonPropertyName("client_id")]
        public string? Client_id { get; set; }

        [JsonPropertyName("client_assertion_type")]
        public string? Client_assertion_type { get; set; }

        [JsonPropertyName("client_assertion")]
        public string? Client_assertion { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
