using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class BrandStatus
    {
        [Key]
        public BrandStatusType BrandStatusId { get; set; }

        [MaxLength(25)]
        [Required]
        public string BrandStatusCode { get; set; }
    }
}
