using System;
using System.Collections.Generic;

namespace CDR.Register.Admin.API.Business.Model
{
    public class LegalEntity
    {
        public Guid LegalEntityId { get; set; }

        public string LegalEntityName { get; set; } = string.Empty;

        public string? AccreditationNumber { get; set; } = null;

        public string? AccreditationLevel { get; set; } = null;

        public string LogoUri { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; } = null;

        public DateTime? RegistrationDate { get; set; }

        public string? RegisteredCountry { get; set; } = null;

        public string? Abn { get; set; } = null;

        public string? Acn { get; set; } = null;

        public string Arbn { get; set; } = string.Empty;

        public string? AnzsicDivision { get; set; } = null;

        public string? OrganisationType { get; set; } = null;

        public virtual ICollection<Brand> DataRecipientBrands { get; set; } = [];
        
    }
}
