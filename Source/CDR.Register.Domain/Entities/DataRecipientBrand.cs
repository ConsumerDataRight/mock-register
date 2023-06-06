using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientBrand : Brand
    {
        public DataRecipient DataRecipient { get; set; }
        public ICollection<SoftwareProduct> SoftwareProducts { get; set; }
    }
}
