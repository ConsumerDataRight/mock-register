using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<ResponseRegisterDataHolderBrandListV1> GetDataHolderBrandsAsyncV1(IndustryEnum industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(IndustryEnum industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataRecipientListV1> GetDataRecipientsAsyncV1(IndustryEnum industry);
        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync();
    }
}
