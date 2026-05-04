using System;
using System.Collections.Generic;

namespace CDR.Register.Discovery.API.Business.Models
{
    public class RegisterDataHolderBrandV2 : IRegisterDataHolderBrand
    {
        public string DataHolderBrandId { get; set; }

        public string BrandName { get; set; }

        public List<string> Industries { get; set; }

        public string LogoUri { get; set; }

        public DataHolderLegalEntityModel LegalEntity { get; set; }

        public string Status { get; set; }

        public AuthDetailModel[] AuthDetails { get; set; }

        public DateTime LastUpdated { get; set; }

        public string BrandGroup { get; set; }

        public RegisterDataHolderBrandServiceEndpointV2 EndpointDetail { get; set; }
    }
}
