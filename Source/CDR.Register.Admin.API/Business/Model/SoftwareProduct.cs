using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Admin.API.Business.Model
{
    public class SoftwareProduct
    {
        [Required]
        public Guid SoftwareProductId { get; set; } = Guid.Empty;

        public string SoftwareProductName { get; set; } = string.Empty;

        public string? SoftwareProductDescription { get; set; } = null;

        public string LogoUri { get; set; } = string.Empty;

        public string ClientUri { get; set; } = string.Empty;

        public string? SectorIdentifierUri { get; set; } = null;

        public string? TosUri { get; set; } = null;

        public string? PolicyUri { get; set; } = null;

        public string? RecipientBaseUri { get; set; } = null;

        public string RevocationUri { get; set; } = string.Empty;

        public string[] RedirectUris { get; set; } = Array.Empty<string>();

        public string JwksUri { get; set; } = string.Empty;

        public string? Scope { get; set; } = null;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public ICollection<SoftwareProductCertificate> Certificates { get; set; } = new List<SoftwareProductCertificate>();
    }
}
