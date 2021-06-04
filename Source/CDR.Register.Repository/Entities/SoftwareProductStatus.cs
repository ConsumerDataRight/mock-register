using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class SoftwareProductStatus
    {
        [Key]
        public SoftwareProductStatusEnum SoftwareProductStatusId { get; set; }
        [MaxLength(25), Required]
        public string SoftwareProductStatusCode { get; set; }
    }

    public enum SoftwareProductStatusEnum : int
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        Removed = 3
    }
}