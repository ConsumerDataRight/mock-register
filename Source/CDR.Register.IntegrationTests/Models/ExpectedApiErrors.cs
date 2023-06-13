using Newtonsoft.Json;
using System.Collections.Generic;

namespace CDR.Register.IntegrationTests.Models
{
    public class ExpectedApiErrors
    {

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("meta")]
        public MetaData Meta { get; set; }
        
        public class MetaData
        {
        }

    }
    
}
