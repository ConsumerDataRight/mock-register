using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientBrand : Brand
    {
        public DataRecipientV1 DataRecipient { get; set; }
        public IList<SoftwareProduct> SoftwareProducts { get; set; }
    }
}
