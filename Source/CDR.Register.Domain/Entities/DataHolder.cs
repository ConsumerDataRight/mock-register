using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.Domain.Entities
{
    public class DataHolderV1
    {
        public Guid DataHolderId { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Industry { get; set; }
        public LegalEntityV1 LegalEntity { get; set; }
        public IList<DataHolderBrandV1> DataHolderBrands { get; set; }

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

    // NB: This is required for V1 backward compatibility and to bridge between (Industry) and future versions using (Industries)
    public class DataHolderV1Tmp
    {
        public Guid DataHolderId { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Industry { get; set; }
        public List<string> Industries { get; set; }
        public LegalEntityDataHolder LegalEntity { get; set; }
        public IList<DataHolderBrandTempV1> Brands { get; set; }

        public DateTime? LastUpdated
        {
            get
            {
                return this.Brands != null && this.Brands.Any()
                    ? this.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                    : null;
            }
        }
    }
}