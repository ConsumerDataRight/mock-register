using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class RegisterUType
    {
        [Key]
        public RegisterUTypes RegisterUTypeId { get; set; }
        [MaxLength(25), Required]
        public string RegisterUTypeCode { get; set; }
    }

    public enum RegisterUTypes
    {
        Unknown = 0,
        SignedJwt = 1
    }
}
