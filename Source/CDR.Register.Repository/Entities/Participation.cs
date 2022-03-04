using CDR.Register.Repository.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class Participation
    {
        public Participation()
        {
            this.ParticipationId = Guid.NewGuid();
        }

        [Key]
        public Guid ParticipationId { get; set; }

        public Guid LegalEntityId { get; set; }
        public LegalEntity LegalEntity { get; set; }

        public ParticipationTypeEnum ParticipationTypeId { get; set; }
        public ParticipationType ParticipationType { get; set; }

        public IndustryEnum? IndustryId { get; set; }
        public IndustryType Industry { get; set; }

        public ParticipationStatusEnum StatusId { get; set; }
        public ParticipationStatus Status { get; set; }

        public virtual ICollection<Brand> Brands { get; set; }
    }
}
