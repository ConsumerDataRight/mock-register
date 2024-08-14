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
        public List<string> Industries { get; set; }
        public DataHolderLegalEntity LegalEntity { get; set; }
        public IList<DataHolderBrand> Brands { get; set; }

        public DateTime? LastUpdated
        {
            get
            {
                return this.Brands != null && this.Brands.Any()
                    ? DateTime.SpecifyKind(this.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated, DateTimeKind.Utc)
                    : null;
            }
        }
    }

    public enum Industry
    {
        All = 0,
        Banking,
        Energy,
        Telco
    }

    public enum DhParticipationStatus
    {
        Unknown = 0,
        Active = 1,
        Removed = 2,
        Inactive = 6
    }
}