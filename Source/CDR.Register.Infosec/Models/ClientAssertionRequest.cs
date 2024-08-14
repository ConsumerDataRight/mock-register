using Newtonsoft.Json;

namespace CDR.Register.Infosec.Models
{
    public class ClientAssertionRequest
    {
        public string? grant_type { get; set; }

        public string? client_id { get; set; }

        public string? client_assertion_type { get; set; }

        [JsonProperty(nameof(client_assertion))]
        public string? client_assertion { get; set; }

        [JsonProperty(nameof(scope))]
        public string? scope { get; set; }
    }
}
