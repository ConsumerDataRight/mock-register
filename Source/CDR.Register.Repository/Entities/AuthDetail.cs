using System;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class AuthDetail
    {
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        public RegisterUTypes RegisterUTypeId { get; set; }
        public RegisterUType RegisterUType { get; set; }
        [MaxLength(1000), Required]
        public string JwksEndpoint { get; set; }
    }
}
