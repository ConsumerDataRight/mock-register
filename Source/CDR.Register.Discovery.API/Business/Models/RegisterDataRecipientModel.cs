using System;

namespace CDR.Register.Discovery.API.Business.Models
{
    public class RegisterDataRecipientModel
    {
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string Industry { get; set; }
        public string LogoUri { get; set; }
        public string Status { get; set; }
        public DataRecipientBrandModel[] DataRecipientBrands { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
