using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    public class ResponseRegisterDataRecipientList
    {
        public IEnumerable<RegisterDataRecipientModel> Data { get; set; }
    }

    public class ResponseRegisterDataRecipientListV2
    {
        public IEnumerable<RegisterDataRecipientModelV2> Data { get; set; }
    }

    public class ResponseRegisterDataRecipientListV3
    {
        public IEnumerable<RegisterDataRecipientModelV3> Data { get; set; }
        public Links Links { get; set; } = new Links();
        public Meta Meta { get; set; } = new Meta();
    }
}