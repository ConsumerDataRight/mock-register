using CDR.Register.Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Admin.API.Business.Model
{
    public class Brand
    {
        public Guid DataRecipientBrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string LogoUri { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public virtual ICollection<SoftwareProduct> SoftwareProducts { get; set; } = [];
    }
}
