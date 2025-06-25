using System.Collections.Generic;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class ExpectedDataRecipients_V2
    {
        public ExpectedDataRecipients_V2()
        {
            this.Data = new List<DataRecipient_V2>();
        }

        [JsonProperty("data")]
        public List<DataRecipient_V2> Data { get; set; }
    }
}
