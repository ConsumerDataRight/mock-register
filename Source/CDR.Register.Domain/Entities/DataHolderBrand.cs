using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class DataHolderBrand : Brand
    {
        public DataHolder DataHolder { get; set; }
        public IList<DataHolderAuthentication> DataHolderAuthentications { get; set; }
        public DataHolderBrandServiceEndpoint DataHolderBrandServiceEndpoint { get; set; }
    }
}
