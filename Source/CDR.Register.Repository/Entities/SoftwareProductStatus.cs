using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class SoftwareProductStatus
    {
        [Key]
        public SoftwareProductStatusType SoftwareProductStatusId { get; set; }
        [MaxLength(25), Required]
        public string SoftwareProductStatusCode { get; set; }
    }

    public enum SoftwareProductStatusType
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        Removed = 3
    }
}