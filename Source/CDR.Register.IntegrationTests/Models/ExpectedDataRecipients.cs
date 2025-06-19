using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Models
{
    public class ExpectedDataRecipients
    {
        public ExpectedDataRecipients()
        {
            this.Data = new List<DataRecipient>();
        }

        [JsonProperty("data")]
        public List<DataRecipient> Data { get; set; }
    }
}
