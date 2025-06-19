using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class SoftwareProduct
    {
        [JsonProperty("softwareProductId")]
        public string SoftwareProductId { get; set; }

        [JsonProperty("softwareProductName")]
        public string SoftwareProductName { get; set; }

        [JsonProperty("softwareProductDescription")]
        public string SoftwareProductDescription { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("recipientBaseUri")]
        public string RecipientBaseUri { get; set; }

        [JsonProperty("redirectUri")]
        public string RedirectUri { get; set; }

        [JsonProperty("jwksUri")]
        public string JwksUri { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
