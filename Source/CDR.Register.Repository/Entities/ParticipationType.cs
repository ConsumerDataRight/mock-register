using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationType
    {
        [Key]
        public ParticipationTypeEnum ParticipationTypeId { get; set; }

        [MaxLength(2), Required]
        public string ParticipationTypeCode { get; set; }
    }

    public enum ParticipationTypeEnum : int
    {
        Unknown = 0,
        Dh = 1,
        Dr = 2
    }
}