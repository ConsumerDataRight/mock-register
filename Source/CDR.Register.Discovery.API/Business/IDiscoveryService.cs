using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsyncXV1(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataHolderBrandListV2> GetDataHolderBrandsAsyncXV2(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsyncXV1(Industry industry);
        Task<ResponseRegisterDataRecipientListV2> GetDataRecipientsAsyncXV2(Industry industry);
        Task<ResponseRegisterDataRecipientListV3> GetDataRecipientsAsyncXV3(Industry industry);
    }
}
