using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    public class ResponseRegisterDataHolderBrandListV2 : IResponseRegisterDataHolderBrandList<RegisterDataHolderBrandV2>
    {
        public IEnumerable<RegisterDataHolderBrandV2> Data { get; set; }

        public LinksPaginated Links { get; set; } = new LinksPaginated();

        public MetaPaginated Meta { get; set; } = new MetaPaginated();
    }
}
