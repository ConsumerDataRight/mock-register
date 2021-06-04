using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.Domain.Entities
{
    public class DataHolder
    {
        public Guid DataHolderId { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Industry { get; set; }
        public LegalEntity LegalEntity { get; set; }
        public IList<DataHolderBrand> DataHolderBrands { get; set; }

        public DateTime? LastUpdated
        {
            get
            {
                return this.DataHolderBrands != null && this.DataHolderBrands.Any()
                    ? this.DataHolderBrands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                    : null;
            }
        }

    }
}
