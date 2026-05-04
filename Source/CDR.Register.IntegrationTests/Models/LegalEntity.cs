using System;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models;

public class LegalEntity
{
    [JsonProperty("legalEntityId")]
    public Guid LegalEntityId { get; set; }

    [JsonProperty("legalEntityName")]
    public string LegalEntityName { get; set; }

    [JsonProperty("logoUri")]
    public Uri LogoUri { get; set; }

    [JsonProperty("registrationNumber")]
    public string RegistrationNumber { get; set; }

    [JsonProperty("registrationDate")]
    public DateTimeOffset? RegistrationDate { get; set; }

    [JsonProperty("registeredCountry")]
    public string RegisteredCountry { get; set; }

    [JsonProperty("abn")]
    public string Abn { get; set; }

    [JsonProperty("acn")]
    public string Acn { get; set; }

    [JsonProperty("anzsicDivision")]
    public string AnzsicDivision { get; set; }

    [JsonProperty("organisationTypeId")]
    public int? OrganisationTypeId { get; set; }

    [JsonProperty("participations")]
    public Participation[] Participations { get; set; }

    [JsonProperty("arbn")]
    public string Arbn { get; set; }

    [JsonProperty("accreditationNumber", NullValueHandling = NullValueHandling.Ignore)]
    public string AccreditationNumber { get; set; }

    [JsonProperty("accreditationLevelId", NullValueHandling = NullValueHandling.Ignore)]
    public int? AccreditationLevelId { get; set; }

    public class Participation
    {
        [JsonProperty("participationId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ParticipationId { get; set; }

        [JsonProperty("participationTypeId")]
        public int ParticipationTypeId { get; set; }

        [JsonProperty("industryId")]
        public int? IndustryId { get; set; }

        [JsonProperty("statusId")]
        public int StatusId { get; set; }

        [JsonProperty("brands")]
        public Brand[] Brands { get; set; }

        public class Brand
        {
            [JsonProperty("brandId")]
            public Guid BrandId { get; set; }

            [JsonProperty("brandName")]
            public string BrandName { get; set; }

            [JsonProperty("brandGroup")]
            public string BrandGroup { get; set; }

            [JsonProperty("logoUri")]
            public Uri LogoUri { get; set; }

            [JsonProperty("brandStatusId")]
            public int BrandStatusId { get; set; }

            [JsonProperty("lastUpdated")]
            public DateTimeOffset LastUpdated { get; set; }

            [JsonProperty("softwareProducts")]
            public SoftwareProduct[] SoftwareProducts { get; set; } = [];

            [JsonProperty("authDetails", NullValueHandling = NullValueHandling.Ignore)]
            public AuthDetail[] AuthDetails { get; set; } = [];

            [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
            public Endpoint Endpoints { get; set; }

            public class AuthDetail
            {
                [JsonProperty("registerUTypeId")]
                public int RegisterUTypeId { get; set; }

                [JsonProperty("jwksEndpoint")]
                public Uri JwksEndpoint { get; set; }
            }

            public class Endpoint
            {
                [JsonProperty("version")]
                public string Version { get; set; }

                [JsonProperty("publicBaseUri")]
                public Uri PublicBaseUri { get; set; }

                [JsonProperty("productBaseUri")]
                public Uri ProductBaseUri { get; set; }

                [JsonProperty("resourceBaseUri")]
                public Uri ResourceBaseUri { get; set; }

                [JsonProperty("infosecBaseUri")]
                public Uri InfosecBaseUri { get; set; }

                [JsonProperty("extensionBaseUri")]
                public string ExtensionBaseUri { get; set; }

                [JsonProperty("websiteUri")]
                public Uri WebsiteUri { get; set; }
            }

            public class SoftwareProduct
            {
                [JsonProperty("softwareProductId")]
                public Guid SoftwareProductId { get; set; }

                [JsonProperty("softwareProductName")]
                public string SoftwareProductName { get; set; }

                [JsonProperty("softwareProductDescription")]
                public string SoftwareProductDescription { get; set; }

                [JsonProperty("logoUri")]
                public Uri LogoUri { get; set; }

                [JsonProperty("sectorIdentifierUri")]
                public Uri SectorIdentifierUri { get; set; }

                [JsonProperty("clientUri")]
                public Uri ClientUri { get; set; }

                [JsonProperty("tosUri")]
                public Uri TosUri { get; set; }

                [JsonProperty("policyUri")]
                public Uri PolicyUri { get; set; }

                [JsonProperty("recipientBaseUri")]
                public Uri RecipientBaseUri { get; set; }

                [JsonProperty("revocationUri")]
                public Uri RevocationUri { get; set; }

                [JsonProperty("redirectUris")]
                public Uri RedirectUris { get; set; }

                [JsonProperty("jwksUri")]
                public Uri JwksUri { get; set; }

                [JsonProperty("scope")]
                public string Scope { get; set; }

                [JsonProperty("statusId")]
                public int StatusId { get; set; }

                [JsonProperty("certificates")]
                public Certificate[] Certificates { get; set; }

                public class Certificate
                {
                    [JsonProperty("commonName")]
                    public string CommonName { get; set; }

                    [JsonProperty("thumbprint")]
                    public string Thumbprint { get; set; }
                }
            }
        }
    }
}
