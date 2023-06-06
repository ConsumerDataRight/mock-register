using CDR.Register.Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Admin.API.Business.Model
{
    public class Brand
    {
        public Guid DataRecipientBrandId { get; set; }

        public string BrandName { get; set; }

        public string LogoUri { get; set; }

        public string Status { get; set; }

        public virtual ICollection<SoftwareProduct> SoftwareProducts { get; set; }
    }
}
