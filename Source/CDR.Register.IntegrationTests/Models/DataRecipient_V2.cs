using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class DataRecipient_V2
    {
        public DataRecipient_V2()
        {
            this.DataRecipientBrands = new List<DataRecipientBrand>();
        }

        [JsonProperty("accreditationNumber")]
        public string AccreditationNumber { get; set; }

        [JsonProperty("legalEntityId")]
        public string LegalEntityId { get; set; }

        [JsonProperty("legalEntityName")]
        public string LegalEntityName { get; set; }

        [JsonProperty("industry")]
        public string[] Industry { get; set; }

        [JsonProperty("logoUri")]
        public string LogoUri { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("dataRecipientBrands")]
        public List<DataRecipientBrand> DataRecipientBrands { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }
    }
}
