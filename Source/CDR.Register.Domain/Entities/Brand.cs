using System;

namespace CDR.Register.Domain.Entities
{
    public abstract class Brand
    {
        private DateTime lastUpdated;
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string LogoUri { get; set; }
        public string BrandStatus { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdated { get => DateTime.SpecifyKind(lastUpdated, DateTimeKind.Utc); set => lastUpdated = value; }
    }
}
