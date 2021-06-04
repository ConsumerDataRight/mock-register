using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class IndustryType
    {
        [Key]
        public IndustryTypeEnum IndustryTypeId { get; set; }

        [MaxLength(25), Required]
        public string IndustryTypeCode { get; set; }
    }

    public enum IndustryTypeEnum : int
    {
        Unknown = 0,
        Banking = 1
    }
}