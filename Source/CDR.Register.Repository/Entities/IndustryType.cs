using CDR.Register.Repository.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class IndustryType
    {
        [Key]
        public IndustryEnum IndustryTypeId { get; set; }

        [MaxLength(25), Required]
        public string IndustryTypeCode { get; set; }
    }
}