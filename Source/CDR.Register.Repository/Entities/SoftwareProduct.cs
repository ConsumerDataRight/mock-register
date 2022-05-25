using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class SoftwareProduct
    {
        public SoftwareProduct()
        {
            // Populate any default values to make the creation easy.
            this.SoftwareProductId = Guid.NewGuid();
            this.Scope = "openid profile common:customer.basic:read common:customer.detail:read bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:regular_payments:read bank:payees:read energy:accounts.basic:read energy:accounts.detail:read energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:billing:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.der:read energy:electricity.usage:read cdr:registration";
        }

        [Key]
        public Guid SoftwareProductId { get; set; }

        [MaxLength(100), Required]
        public string SoftwareProductName { get; set; }

        [MaxLength(1000)]
        public string SoftwareProductDescription { get; set; }

        [MaxLength(1000), Required]
        public string LogoUri { get; set; }

        [MaxLength(1000)]
        public string SectorIdentifierUri { get; set; }

        [MaxLength(1000), Required]
        public string ClientUri { get; set; }

        [MaxLength(1000)]
        public string TosUri { get; set; }

        [MaxLength(1000)]
        public string PolicyUri { get; set; }

        [MaxLength(1000)]
        public string RecipientBaseUri { get; set; }

        [MaxLength(1000), Required]
        public string RevocationUri { get; set; }

        [MaxLength(2000), Required]
        public string RedirectUris { get; set; }

        [MaxLength(1000), Required]
        public string JwksUri { get; set; }

        [MaxLength(1000), Required]
        public string Scope { get; set; }

        public SoftwareProductStatusType StatusId { get; set; }
        public SoftwareProductStatus Status { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        public ICollection<SoftwareProductCertificate> Certificates { get; set; }
    }
}
