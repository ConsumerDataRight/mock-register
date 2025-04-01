using System;

namespace CDR.Register.Admin.API.Business.Model
{
    public class DataHolderLegalEntityModel
    {
        public Guid LegalEntityId { get; set; } = Guid.Empty;

        public string LegalEntityName { get; set; } = string.Empty;

        public string LogoUri { get; set; } = string.Empty;

        public string? RegistrationNumber { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public string? RegisteredCountry { get; set; }

        public string? Abn { get; set; }

        public string? Acn { get; set; }

        public string? Arbn { get; set; }

        public string? AnzsicDivision { get; set; }

        public string? OrganisationType { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
