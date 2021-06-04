using System.ComponentModel.DataAnnotations;

namespace CDR.Register.SSA.API.Business.Models
{
    public class SoftwareStatementAssertionV2Model : SoftwareStatementAssertionModel
    {
        /// <summary>
        /// Base URI for the Consumer Data Standard data recipient endpoints. This should be the base to provide reference to all other Data Recipient Endpoints
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string recipient_base_uri { get; set; }

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
}
