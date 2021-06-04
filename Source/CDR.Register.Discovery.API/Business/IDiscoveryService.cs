using System;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync(Industry industry);
        Task<ResponseRegisterDataRecipientListV2> GetDataRecipientsV2Async(Industry industry);
    }
}
