using System.Text.Json.Serialization;

namespace CDR.Register.IntegrationTests.Models
{
    /// <summary>
    /// Access token.
    /// </summary>
    public class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string Access_token { get; set; }

        [JsonPropertyName("expires_in")]
        public int Expires_in { get; set; }

        [JsonPropertyName("token_type")]
        public string Token_type { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
