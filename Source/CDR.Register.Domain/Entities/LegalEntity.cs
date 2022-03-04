using System;

namespace CDR.Register.Domain.Entities
{
    public class LegalEntityV1
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string AccreditationNumber { get; set; }
        public string IndustryCode { get; set; }
        public string LogoUri { get; set; }
        public string OrganisationType { get; set; }
        public string Status { get; set; }
        private DateTime? registrationDate;
        public string RegistrationNumber { get; set; }
        public DateTime? RegistrationDate { get => registrationDate?.Date; set => registrationDate = value; }
        public string RegisteredCountry { get; set; }
        public string Abn { get; set; }
        public string Acn { get; set; }
        public string Arbn { get; set; }
    }

    public class LegalEntityDataHolder
    {
        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LogoUri { get; set; }
        public string OrganisationType { get; set; }
        public string Status { get; set; }
        private DateTime? registrationDate;
        public string RegistrationNumber { get; set; }
        public DateTime? RegistrationDate { get => registrationDate?.Date; set => registrationDate = value; }
        public string RegisteredCountry { get; set; }
        public string Abn { get; set; }
        public string Acn { get; set; }
        public string Arbn { get; set; }
        public string IndustryCode { get; set; }
        public string AccreditationNumber { get; set; }
        public string AnzsicDivision { get; set; }
    }

    public class LegalEntityDataRecipient
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