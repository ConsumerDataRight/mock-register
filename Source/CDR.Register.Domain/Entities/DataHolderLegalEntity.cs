using System;

namespace CDR.Register.Domain.Entities
{
    public class DataHolderLegalEntity
    {
        private DateTime? _registrationDate;

        public Guid LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LogoUri { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? RegistrationDate { get => _registrationDate?.Date; set => _registrationDate = value; }
        public string RegisteredCountry { get; set; }
        public string Abn { get; set; }
        public string Acn { get; set; }
        public string Arbn { get; set; }
        public string AnzsicDivision { get; set; }
        public string OrganisationType { get; set; }
        public string Status { get; set; }
    }

    public enum DhStatus
    {
        Active = 1,
        Removed = 2,
        Inactive = 6
    }

    public enum OrganisationType
    {
        Unknown = 0,
        SoleTrader = 1,
        Company = 2,
        Partnership = 3,
        Trust = 4,
        GovernmentEntity = 5,
        Other = 6
    }
}