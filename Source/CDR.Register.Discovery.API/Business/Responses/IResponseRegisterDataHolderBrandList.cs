using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;

namespace CDR.Register.Discovery.API.Business.Responses
{
    /// <summary>
    /// Response with list of DH Brands.
    /// </summary>
    /// <typeparam name="T">The DH Brand collection.</typeparam>
    public interface IResponseRegisterDataHolderBrandList<out T>
        where T : IRegisterDataHolderBrand
    {
        IEnumerable<T> Data { get; }

        LinksPaginated Links { get; set; }

        MetaPaginated Meta { get; set; }
    }
}
