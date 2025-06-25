using System.Collections.Generic;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class DataHolderMetadata
    {
        [JsonProperty("dataHolderBrandId")]
        public string DataHolderBrandId { get; set; }

        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("industries")]
        public List<string> Industries { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("endpointDetail")]
        public EndpointDetailChild EndpointDetail { get; set; }

        [JsonProperty("authDetails")]
        public AuthDetailsChild AuthDetails { get; set; }

        [JsonProperty("legalEntity")]
        public LegalEntityChild LegalEntity { get; set; }

        public class AuthDetailsChild
        {
            [JsonProperty("registerUType")]
            public string RegisterUType { get; set; }

            [JsonProperty("jwksEndpoint")]
            public string JwksEndpoint { get; set; }
        }

        public class EndpointDetailChild
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("publicBaseUri")]
            public string PublicBaseUri { get; set; }

            [JsonProperty("resourceBaseUri")]
            public string ResourceBaseUri { get; set; }

            [JsonProperty("infosecBaseUri")]
            public string InfosecBaseUri { get; set; }

            [JsonProperty("extensionBaseUri")]
            public string ExtensionBaseUri { get; set; }

            [JsonProperty("websiteUri")]
            public string WebsiteUri { get; set; }
        }

        public class LegalEntityChild
        {
            [JsonProperty("legalEntityId")]
            public string LegalEntityId { get; set; }

            [JsonProperty("legalEntityName")]
            public string LegalEntityName { get; set; }

            [JsonProperty("logoUri")]
            public string LogoUri { get; set; }

            [JsonProperty("registrationNumber")]
            public string RegistrationNumber { get; set; }

            [JsonProperty("registrationDate")]
            public string RegistrationDate { get; set; }

            [JsonProperty("registeredCountry")]
            public string RegisteredCountry { get; set; }

            [JsonProperty("abn")]
            public string Abn { get; set; }

            [JsonProperty("acn")]
            public string Acn { get; set; }

            [JsonProperty("arbn")]
            public string Arbn { get; set; }

            [JsonProperty("anzsicDivision")]
            public string AnzsicDivision { get; set; }

            [JsonProperty("organisationType")]
            public string OrganisationType { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }
        }
    }
}
