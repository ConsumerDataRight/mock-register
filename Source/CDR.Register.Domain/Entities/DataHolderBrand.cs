using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class DataHolderBrandV1 : Brand
    {
        public DataHolderV1 DataHolder { get; set; }
        public IList<DataHolderAuthentication> DataHolderAuthentications { get; set; }
        public DataHolderBrandServiceEndpoint DataHolderBrandServiceEndpoint { get; set; }
    }

    public class DataHolderBrandTempV1 : Brand
    {
        // NB: This is required for V1 backward compatibility and to bridge between (Industry) and future versions using (Industries)
    }

    public class DataHolderBrand : Brand
    {
        // NB: This is required for V1 backward compatibility to bridge between (Industry) and future versions using (Industries)
        public DataHolderV1Tmp DataHolder { get; set; }
        public IList<DataHolderAuthentication> DataHolderAuthentications { get; set; }
        public DataHolderBrandServiceEndpoint DataHolderBrandServiceEndpoint { get; set; }
    }
}