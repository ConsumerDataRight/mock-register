using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<ResponseRegisterDataHolderBrandListV1> GetDataHolderBrandsAsyncV1(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataRecipientListV1> GetDataRecipientsAsyncV1(Industry industry);
        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync();
    }
}
