using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationType
    {
        [Key]
        public ParticipationTypes ParticipationTypeId { get; set; }

        [MaxLength(2)]
        [Required]
        public string ParticipationTypeCode { get; set; }
    }
}
