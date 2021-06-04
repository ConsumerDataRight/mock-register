using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationStatus
    {
        [Key]
        public ParticipationStatusEnum ParticipationStatusId { get; set; }

        [MaxLength(25), Required]
        public string ParticipationStatusCode { get; set; }
    }

    public enum ParticipationStatusEnum : int
    {
        Unknown = 0,
        Active = 1,
        Removed = 2,
        Suspended = 3,
        Revoked = 4,
        Surrendered = 5,
        Inactive = 6
    }
}