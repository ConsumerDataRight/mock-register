using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterDiscoveryRepository
    {
        Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsyncXV1(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<Page<DataHolderBrandV2[]>> GetDataHolderBrandsAsyncXV2(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<DataRecipient[]> GetDataRecipientsAsyncXV1(Industry industry);
        Task<DataRecipientV2[]> GetDataRecipientsAsyncXV2(Industry industry);
        Task<DataRecipientV3[]> GetDataRecipientsAsyncXV3(Industry industry);
        Task<SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId);
    }
}
