using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CDR.Register.SSA.API.Business.Models
{
    public class SoftwareStatementAssertionModel
    {
        /// <summary>
        /// Gets or sets iss (issuer) claim denoting the party attesting to the claims in the software statement.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("iss")]
        public string Iss { get; set; }

        /// <summary>
        /// Gets or sets iat (issued at) claim.
        /// </summary>
        [Required]
        [JsonPropertyName("iat")]
        public long Iat { get; set; }

        /// <summary>
        /// Gets or sets exp (expiration time) claim
        /// MUST NOT be accepted for processing.
        /// </summary>
        [Required]
        [JsonPropertyName("exp")]
        public long Exp { get; set; }

        /// <summary>
        /// Gets or sets jti (JWT ID) claim.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("jti")]
        public string Jti { get; set; }

        /// <summary>
        /// Gets or sets unique identifier string assigned by the CDR Register that identifies CDR Participant (e.g. ADR).
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("org_id")]
        public string Org_id { get; set; }

        /// <summary>
        /// Gets or sets human-readable string name of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("org_name")]
        public string Org_name { get; set; }

        /// <summary>
        /// Gets or sets human-readable string name of the software product to be presented to the end-user during authorization.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("client_name")]
        public string Client_name { get; set; }

        /// <summary>
        /// Gets or sets human-readable string name of the software product description to be presented to the end user during authorization.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("client_description")]
        public string Client_description { get; set; }

        /// <summary>
        /// Gets or sets URL string of a web page providing information about the ADR Software Product.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("client_uri")]
        public string Client_uri { get; set; }

        /// <summary>
        /// Gets or sets array of redirection URI strings for use in redirect-based flows.
        /// </summary>
        [Required]
        [JsonPropertyName("redirect_uris")]
        public IEnumerable<string> Redirect_uris { get; set; }

        /// <summary>
        /// Gets or sets URL string that references a logo for the client. If present, the server SHOULD display this image to the end-user during approval.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("logo_uri")]
        public string Logo_uri { get; set; }

        /// <summary>
        /// Gets or sets URL string that points to a humanreadable terms of service document for the Software Product.
        /// </summary>
        [JsonPropertyName("tos_uri")]
        public string Tos_uri { get; set; }

        /// <summary>
        /// Gets or sets URL string that points to a humanreadable policy document for the Software Product.
        /// </summary>
        [JsonPropertyName("policy_uri")]
        public string Policy_uri { get; set; }

        /// <summary>
        /// Gets or sets URL string referencing the client's JSON Web Key (JWK) Set [RFC7517] document, which contains the client's public keys.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("jwks_uri")]
        public string Jwks_uri { get; set; }

        /// <summary>
        /// Gets or sets URI string that references the location of the Software Product consent revocation endpoint as per https://consumerdatastandardsaustralia.github.io/standards/#end-points.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("revocation_uri")]
        public string Revocation_uri { get; set; }

        /// <summary>
        /// Gets or sets base URI for the Consumer Data Standard data recipient endpoints. This should be the base to provide reference to all other Data Recipient Endpoints.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("recipient_base_uri")]
        public string Recipient_base_uri { get; set; }

        /// <summary>
        /// Gets or sets string representing a unique identifier assigned by the ACCC Register and used by registration endpoints to identify the software product to be dynamically registered.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("software_id")]
        public string Software_id { get; set; }

        /// <summary>
        /// Gets or sets string containing a role of the software in thwe CDR Regime. Initially the only value used with be "data-recipient-software-product".
        /// </summary>
        [JsonPropertyName("software_roles")]
        public string Software_roles { get; set; }

        /// <summary>
        /// Gets or sets string containing a space-separated list of scope values that the client can use when requesting access tokens.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets URL string that references a sector uri for the client. If present, the server SHOULD display this image to the end-user during approval.
        /// </summary>
        [JsonPropertyName("sector_identifier_uri")]
        public string Sector_identifier_uri { get; set; }

        /// <summary>
        /// Gets or sets human-readable string legal entity id of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        [JsonPropertyName("legal_entity_id")]
        public string Legal_entity_id { get; set; }

        /// <summary>
        /// Gets or sets human-readable string legal entity name of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        [JsonPropertyName("legal_entity_name")]
        public string Legal_entity_name { get; set; }
    }
}
