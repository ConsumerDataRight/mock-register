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

        public ParticipationTypes ParticipationTypeId { get; set; }
        public ParticipationType ParticipationType { get; set; }

        public Industry? IndustryId { get; set; }
        public IndustryType Industry { get; set; }

        public ParticipationStatusType StatusId { get; set; }
        public ParticipationStatus Status { get; set; }

        public virtual ICollection<Brand> Brands { get; set; }
    }
}
