using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationType
    {
        [Key]
        public ParticipationTypes ParticipationTypeId { get; set; }

        [MaxLength(2), Required]
        public string ParticipationTypeCode { get; set; }
    }

    public enum ParticipationTypes
    {
        Unknown = 0,
        Dh = 1,
        Dr = 2
    }
}