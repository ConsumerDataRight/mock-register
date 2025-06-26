using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipient
    {
        public static string Industry => "banking";

        public Guid DataRecipientId { get; set; }

        public string Status { get; set; }

        public bool IsActive { get; set; }

        public DataRecipientLegalEntity LegalEntity { get; set; }

        public IList<DataRecipientBrand> DataRecipientBrands { get; set; }

        public DateTime? LastUpdated
        {
            get
            {
                return this.DataRecipientBrands != null && this.DataRecipientBrands.Any()
                    ? DateTime.SpecifyKind(this.DataRecipientBrands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated, DateTimeKind.Utc)
                    : null;
            }
        }
    }
}
