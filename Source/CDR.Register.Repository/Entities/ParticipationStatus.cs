using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class ParticipationStatus
    {
        [Key]
        public ParticipationStatusType ParticipationStatusId { get; set; }

        [MaxLength(25), Required]
        public string ParticipationStatusCode { get; set; }

        /// <summary>
        /// Applicable participation types. If null or Unknown, it's available for all participation types.
        /// </summary>
		public ParticipationTypes? ParticipationTypeId { get; set; }
	}

    public enum ParticipationStatusType
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