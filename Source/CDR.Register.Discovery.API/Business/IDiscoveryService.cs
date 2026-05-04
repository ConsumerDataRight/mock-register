using System;
using System.Threading.Tasks;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<IResponseRegisterDataHolderBrandList<IRegisterDataHolderBrand>> GetDataHolderBrands(Industry industry, DateTime? updatedSince, int page, int pageSize, int version);

        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync();
    }
}
