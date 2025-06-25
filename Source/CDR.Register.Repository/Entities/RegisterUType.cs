using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class RegisterUType
    {
        [Key]
        public RegisterUTypes RegisterUTypeId { get; set; }

        [MaxLength(25)]
        [Required]
        public string RegisterUTypeCode { get; set; }
    }
}
