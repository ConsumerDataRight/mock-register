using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Status.API.Business.Models;

namespace CDR.Register.Status.API.Business.Responses
{
    public class ResponseRegisterDataRecipientStatusList
    {
        public IEnumerable<RegisterDataRecipientStatusModel> DataRecipients { get; set; }
    }

    public class ResponseRegisterDataRecipientStatusListV2
    {
        public IEnumerable<RegisterDataRecipientStatusModelV2> Data { get; set; }
        public Links Links { get; set; } = new Links();
        public Meta Meta { get; set; } = new Meta();
    }
}