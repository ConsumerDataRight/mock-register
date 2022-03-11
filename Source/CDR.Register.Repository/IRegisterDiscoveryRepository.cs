using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterDiscoveryRepository
    {
        Task<Page<DataHolderBrandV1[]>> GetDataHolderBrandsAsyncV1(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<DataRecipientV1[]> GetDataRecipientsAsyncV1(Industry industry);
        Task<DataRecipient[]> GetDataRecipientsAsync();
        Task<SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId);
    }
}
