using System;
using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class SoftwareProduct
    {
        public Guid SoftwareProductId { get; set; }
        public string SoftwareProductName { get; set; }
        public string SoftwareProductDescription { get; set; }
        public string LogoUri { get; set; }
        public string SectorIdentifierUri { get; set; }
        public string ClientUri { get; set; }
        public string TosUri { get; set; }
        public string PolicyUri { get; set; }
        public string RecipientBaseUri { get; set; }
        public string RevocationUri { get; set; }
        public string RedirectUri { get; set; }
        public IEnumerable<string> RedirectUris => RedirectUri?.Split(" ");
        public string JwksUri { get; set; }
        public string Scope { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DataRecipientBrand DataRecipientBrand { get; set; }
    }
}