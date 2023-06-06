using Newtonsoft.Json;
using System.Collections.Generic;

namespace CDR.Register.IntegrationTests.Models
{
    public class DataRecipientMetadata
    {
        [JsonProperty("legalEntityId")]
        public string LegalEntityId { get; set; }

        [JsonProperty("legalEntityName")]
        public string LegalEntityName { get; set; }

        [JsonProperty("accreditationNumber")]
        public string AccreditationNumber { get; set; }

        [JsonProperty("accreditationLevel")]
        public string AccreditationLevel { get; set; }

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
        public object AnzsicDivision { get; set; }

        [JsonProperty("organisationType")]
        public string OrganisationType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("dataRecipientBrands")]
        public List<DataRecipientBrand> DataRecipientBrands { get; set; }

        public class Certificate
        {
            [JsonProperty("commonName")]
            public string CommonName { get; set; }

            [JsonProperty("thumbprint")]
            public string Thumbprint { get; set; }
        }

        public class DataRecipientBrand
        {
            [JsonProperty("dataRecipientBrandId")]
            public string DataRecipientBrandId { get; set; }

            [JsonProperty("brandName")]
            public string BrandName { get; set; }

            [JsonProperty("logoUri")]
            public string LogoUri { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("softwareProducts")]
            public List<SoftwareProduct> SoftwareProducts { get; set; }
        }

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

            [JsonProperty("sectorIdentifierUri")]
            public object SectorIdentifierUri { get; set; }

            [JsonProperty("clientUri")]
            public string ClientUri { get; set; }

            [JsonProperty("tosUri")]
            public string TosUri { get; set; }

            [JsonProperty("policyUri")]
            public string PolicyUri { get; set; }

            [JsonProperty("recipientBaseUri")]
            public string RecipientBaseUri { get; set; }

            [JsonProperty("revocationUri")]
            public string RevocationUri { get; set; }

            [JsonProperty("redirectUris")]
            public List<string> RedirectUris { get; set; }

            [JsonProperty("jwksUri")]
            public string JwksUri { get; set; }

            [JsonProperty("scope")]
            public object Scope { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("certificates")]
            public List<Certificate> Certificates { get; set; }
        }


    }
}
