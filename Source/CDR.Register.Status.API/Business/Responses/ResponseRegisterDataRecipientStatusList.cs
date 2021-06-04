using System.Collections.Generic;
using CDR.Register.Status.API.Business.Models;

namespace CDR.Register.Status.API.Business.Responses
{
    public class ResponseRegisterDataRecipientStatusList
    {
        public IEnumerable<RegisterDataRecipientStatusModel> DataRecipients { get; set; }
    }
}
