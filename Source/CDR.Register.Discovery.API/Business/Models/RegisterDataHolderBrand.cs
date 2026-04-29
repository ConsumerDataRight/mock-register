using System;
using System.Collections.Generic;

namespace CDR.Register.Discovery.API.Business.Models
{
    [Obsolete("Deprecated in the standards, used by versions prior to V1.35.0. This is aligned with RAAP implementation and can be removed when the endpoint is no longer supported.", false)]
    public class RegisterDataHolderBrand : IRegisterDataHolderBrand
    {
        public string DataHolderBrandId { get; set; }

        public string BrandName { get; set; }

        public List<string> Industries { get; set; }

        public string LogoUri { get; set; }

        public DataHolderLegalEntityModel LegalEntity { get; set; }

        public string Status { get; set; }

        public AuthDetailModel[] AuthDetails { get; set; }

        public DateTime LastUpdated { get; set; }

        public RegisterDataHolderBrandServiceEndpoint EndpointDetail { get; set; }
    }
}
