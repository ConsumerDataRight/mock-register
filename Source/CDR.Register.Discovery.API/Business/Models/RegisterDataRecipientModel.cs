using System;

namespace CDR.Register.Discovery.API.Business.Models
{
    public class RegisterDataRecipientModel
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string Industry { get; set; }
        public string LogoUri { get; set; }
        public DataRecipientBrandModel[] DataRecipientBrands { get; set; }
        public string Status { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class RegisterDataRecipientModelV2 : RegisterDataRecipientModel
    {
        public string AccreditationNumber { get; set; }
    }

    public class RegisterDataRecipientModelV3
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccreditationNumber { get; set; }
        public string AccreditationLevel { get; set; }
        public string LogoUri { get; set; }
        public DataRecipientBrandModel[] DataRecipientBrands { get; set; }
        public string Status { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}