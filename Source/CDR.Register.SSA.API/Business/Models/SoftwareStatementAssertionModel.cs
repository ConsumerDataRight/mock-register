using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.SSA.API.Business.Models
{
    public class SoftwareStatementAssertionModel
    {
        /// <summary>
        /// iss (issuer) claim denoting the party attesting to the claims in the software statement
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string iss { get; set; }

        /// <summary>
        /// iat (issued at) claim
        /// </summary>
        [Required]
        public long iat { get; set; }

        /// <summary>
        /// exp (expiration time) claim
        /// MUST NOT be accepted for processing
        /// </summary>
        [Required]
        public long exp { get; set; }

        /// <summary>
        /// jti (JWT ID) claim
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string jti { get; set; }

        /// <summary>
        /// A unique identifier string assigned by the CDR Register that identifies CDR Participant (e.g. ADR)
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string org_id { get; set; }

        /// <summary>
        /// Human-readable string name of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string org_name { get; set; }

        /// <summary>
        /// Human-readable string name of the software product to be presented to the end-user during authorization
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string client_name { get; set; }

        /// <summary>
        /// Human-readable string name of the software product description to be presented to the end user during authorization
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string client_description { get; set; }

        /// <summary>
        /// URL string of a web page providing information about the ADR Software Product
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string client_uri { get; set; }

        /// <summary>
        /// Array of redirection URI strings for use in redirect-based flows
        /// </summary>
        [Required]
        public IEnumerable<string> redirect_uris { get; set; }

        /// <summary>
        /// URL string that references a logo for the client. If present, the server SHOULD display this image to the end-user during approval
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string logo_uri { get; set; }

        /// <summary>
        /// URL string that points to a humanreadable terms of service document for the Software Product
        /// </summary>
        public string tos_uri { get; set; }

        /// <summary>
        /// URL string that points to a humanreadable policy document for the Software Product
        /// </summary>
        public string policy_uri { get; set; }

        /// <summary>
        /// URL string referencing the client's JSON Web Key (JWK) Set [RFC7517] document, which contains the client's public keys
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string jwks_uri { get; set; }

        /// <summary>
        /// URI string that references the location of the Software Product consent revocation endpoint as per https://consumerdatastandardsaustralia.github.io/standards/#end-points
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string revocation_uri { get; set; }

        /// <summary>
        /// Base URI for the Consumer Data Standard data recipient endpoints. This should be the base to provide reference to all other Data Recipient Endpoints
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string recipient_base_uri { get; set; }

        /// <summary>
        /// String representing a unique identifier assigned by the ACCC Register and used by registration endpoints to identify the software product to be dynamically registered
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string software_id { get; set; }

        /// <summary>
        /// String containing a role of the software in thwe CDR Regime. Initially the only value used with be "data-recipient-software-product"
        /// </summary>
        public string software_roles { get; set; }

        /// <summary>
        /// String containing a space-separated list of scope values that the client can use when requesting access tokens
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string scope { get; set; }
    }

    public class SoftwareStatementAssertionModelV2 : SoftwareStatementAssertionModel
    {
        /// <summary>
        /// URL string that references a sector uri for the client. If present, the server SHOULD display this image to the end-user during approval
        /// </summary>
        public string sector_identifier_uri { get; set; }

        /// <summary>
        /// Human-readable string legal entity id of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        public string legal_entity_id { get; set; }

        /// <summary>
        /// Human-readable string legal entity name of the Accredited Data Recipient to be presented to the end user during authorization.
        /// </summary>
        public string legal_entity_name { get; set; }
    }

    public class SoftwareStatementAssertionModelV3 : SoftwareStatementAssertionModelV2
    {
    }
}