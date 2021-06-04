using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class LegalEntity
    {
        public LegalEntity()
        {
            this.LegalEntityId = Guid.NewGuid();
        }

        [Key]
        public Guid LegalEntityId { get; set; }

        [MaxLength(100), Required]
        public string LegalEntityName { get; set; }

        [MaxLength(1000), Required]
        public string LogoUri { get; set; }

        [MaxLength(50)]
        public string RegistrationNumber { get; set; }

        public DateTime? RegistrationDate { get; set; }

        [MaxLength(100)]
        public string RegisteredCountry { get; set; }

        [MaxLength(11)]
        public string Abn { get; set; }

        [MaxLength(9)]
        public string Acn { get; set; }

        [MaxLength(9)]
        public string Arbn { get; set; }

        [MaxLength(4)]
        public string IndustryCode { get; set; }

        public OrganisationTypeEnum? OrganisationTypeId { get; set; }
        public OrganisationType OrganisationType { get; set; }

        [MaxLength(25)]
        public string AccreditationNumber { get; set; }

        public virtual ICollection<Participation> Participations { get; set; }

    }
}
