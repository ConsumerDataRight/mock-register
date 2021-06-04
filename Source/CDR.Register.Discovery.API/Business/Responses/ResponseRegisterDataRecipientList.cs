using System.Collections.Generic;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    public class ResponseRegisterDataRecipientList
    {
        public IEnumerable<RegisterDataRecipientModel> Data { get; set; }
    }
}
