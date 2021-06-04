using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class RegisterUType
    {
        [Key]
        public RegisterUTypeEnum RegisterUTypeId { get; set; }
        [MaxLength(25), Required]
        public string RegisterUTypeCode { get; set; }
    }

    public enum RegisterUTypeEnum : int
    {
        Unknown = 0,
        SignedJwt = 1
    }
}
