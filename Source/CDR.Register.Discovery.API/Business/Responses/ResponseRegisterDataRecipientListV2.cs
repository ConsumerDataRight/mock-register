using System.Collections.Generic;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    public class ResponseRegisterDataRecipientListV2
    {
        public IEnumerable<RegisterDataRecipientModelV2> Data { get; set; }
    }
}
