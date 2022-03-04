using System.Collections.Generic;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    public class ResponseRegisterDataRecipientListV1
    {
        public IEnumerable<RegisterDataRecipientModelV1> Data { get; set; }
    }

    public class ResponseRegisterDataRecipientListV2
    {
        public IEnumerable<RegisterDataRecipientV2> Data { get; set; }
    }

    public class ResponseRegisterDataRecipientList
    {
        public IEnumerable<RegisterDataRecipient> Data { get; set; }
    }
}