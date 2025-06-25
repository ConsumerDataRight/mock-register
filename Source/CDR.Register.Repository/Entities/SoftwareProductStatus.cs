using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class SoftwareProductStatus
    {
        [Key]
        public SoftwareProductStatusType SoftwareProductStatusId { get; set; }

        [MaxLength(25)]
        [Required]
        public string SoftwareProductStatusCode { get; set; }
    }
}
