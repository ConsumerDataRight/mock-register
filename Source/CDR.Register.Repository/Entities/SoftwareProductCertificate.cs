using System;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class SoftwareProductCertificate
    {
        [Key]
        public Guid SoftwareProductCertificateId { get; set; }

        public Guid SoftwareProductId { get; set; }

        public SoftwareProduct SoftwareProduct { get; set; }

        [MaxLength(2000), Required]
        public string CommonName { get; set; }

        [MaxLength(2000), Required]
        public string Thumbprint { get; set; }
    }
}
