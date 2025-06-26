using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationStatus
    {
        [Key]
        public ParticipationStatusType ParticipationStatusId { get; set; }

        [MaxLength(25)]
        [Required]
        public string ParticipationStatusCode { get; set; }

        /// <summary>
        /// Gets or sets applicable participation types. If null or Unknown, it's available for all participation types.
        /// </summary>
        public ParticipationTypes? ParticipationTypeId { get; set; }
    }
}
