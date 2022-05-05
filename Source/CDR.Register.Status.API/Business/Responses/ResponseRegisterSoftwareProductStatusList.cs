using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Status.API.Business.Models;

namespace CDR.Register.Status.API.Business.Responses
{
    public class ResponseRegisterSoftwareProductStatusList
    {
        public IEnumerable<RegisterSoftwareProductStatusModel> SoftwareProducts { get; set; }
    }

    public class ResponseRegisterSoftwareProductStatusListV2
    {
        public IEnumerable<RegisterSoftwareProductStatusModelV2> Data { get; set; }
        public Links Links { get; set; } = new Links();
        public Meta Meta { get; set; } = new Meta();
    }
}