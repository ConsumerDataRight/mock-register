using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class Brand
    {
        public Brand()
        {
            this.BrandId = Guid.NewGuid();
        }

        [Key]
        public Guid BrandId { get; set; }
        [MaxLength(100), Required]
        public string BrandName { get; set; }
        [MaxLength(1000), Required]
        public string LogoUri { get; set; }
        public BrandStatusType BrandStatusId { get; set; }
        public BrandStatus BrandStatus { get; set; }
        public Guid ParticipationId { get; set; }
        public Participation Participation { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual ICollection<SoftwareProduct> SoftwareProducts { get; set; }
        public virtual ICollection<AuthDetail> AuthDetails { get; set; }
        public virtual Endpoint Endpoint { get; set; }
    }
}
