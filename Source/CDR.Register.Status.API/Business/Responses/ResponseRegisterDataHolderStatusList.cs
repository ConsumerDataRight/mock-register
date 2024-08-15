using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Domain.Models;
using CDR.Register.Status.API.Business.Models;

namespace CDR.Register.Status.API.Business.Responses
{
    public class ResponseRegisterDataHolderStatusList
    {
        public IEnumerable<RegisterDataHolderStatusModel> Data { get; set; }
        public Links Links { get; set; } = new Links();
        public Meta Meta { get; set; } = new Meta();
    }
}