using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDR.Register.Repository.Entities
{
    public class Endpoint
    {
        [Key]
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }
        [MaxLength(25), Required]
        public string Version { get; set; }
        [MaxLength(500), Required]
        public string PublicBaseUri { get; set; }
        [MaxLength(500), Required]
        public string ResourceBaseUri { get; set; }
        [MaxLength(500), Required]
        public string InfosecBaseUri { get; set; }
        [MaxLength(500)]
        public string ExtensionBaseUri { get; set; }
        [MaxLength(1000), Required]
        public string WebsiteUri { get; set; }
        public virtual Brand Brand { get; set; }

    }
}
