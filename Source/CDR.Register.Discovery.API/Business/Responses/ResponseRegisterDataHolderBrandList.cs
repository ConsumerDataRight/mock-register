using System;
using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    [Obsolete("Deprecated in the standards, used by versions prior to V1.35.0. This is aligned with RAAP implementation and can be removed when the endpoint is no longer supported.", false)]
    public class ResponseRegisterDataHolderBrandList : IResponseRegisterDataHolderBrandList<RegisterDataHolderBrand>
    {
        public IEnumerable<RegisterDataHolderBrand> Data { get; set; }

        public LinksPaginated Links { get; set; } = new LinksPaginated();

        public MetaPaginated Meta { get; set; } = new MetaPaginated();
    }
}
