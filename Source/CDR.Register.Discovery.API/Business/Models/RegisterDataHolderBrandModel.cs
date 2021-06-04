using System;

namespace CDR.Register.Discovery.API.Business.Models
{
    public class RegisterDataHolderBrandModel
    {
        public string DataHolderBrandId { get; set; }
        public string BrandName { get; set; }
        public string LogoUri { get; set; }
        public LegalEntityModel LegalEntity { get; set; }
        public string Status { get; set; }
        public EndpointDetailModel EndPointDetail { get; set; }
        public AuthDetailModel[] AuthDetails { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
