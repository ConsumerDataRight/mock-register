using System;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientLegalEntity
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccreditationNumber { get; set; }
        public string IndustryCode { get; set; }
        public string LogoUri { get; set; }
        public string Status { get; set; }
    }

    public class DataRecipientLegalEntityV2
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LogoUri { get; set; }
        public string OrganisationType { get; set; }
        public string Status { get; set; }
        public string AccreditationNumber { get; set; }
        public string AccreditationLevelId { get; set; }
    }
}