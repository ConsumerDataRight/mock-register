using System.Collections.Generic;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class DataRecipientBrand
    {
        public DataRecipientBrand()
        {
            this.SoftwareProducts = new List<SoftwareProduct>();
        }

        [JsonProperty("dataRecipientBrandId")]
        public string DataRecipientBrandId { get; set; }

        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("softwareProducts")]
        public List<SoftwareProduct> SoftwareProducts { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
