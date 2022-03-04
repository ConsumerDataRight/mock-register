using System;
using System.Collections.Generic;

namespace CDR.Register.Discovery.API.Business.Models
{
    public class RegisterDataHolderBrandModelV1
    {
        public string DataHolderBrandId { get; set; }
        public string BrandName { get; set; }
        public string Industry { get; set; }
        public string LogoUri { get; set; }
        public LegalEntityModelV1 LegalEntity { get; set; }
        public string Status { get; set; }
        public EndpointDetailModel EndPointDetail { get; set; }
        public AuthDetailModel[] AuthDetails { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class RegisterDataHolderBrandModel
    {
        public string DataHolderBrandId { get; set; }
        public string BrandName { get; set; }
        public List<string> Industries { get; set; }
        public string LogoUri { get; set; }
        public LegalEntityDataHolderModel LegalEntity { get; set; }
        public string Status { get; set; }
        public EndpointDetailModel EndPointDetail { get; set; }
        public AuthDetailModel[] AuthDetails { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}