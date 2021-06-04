using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;

namespace CDR.Register.Domain.Repositories
{
    public interface IRegisterDiscoveryRepository
    {
        Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<DataRecipient[]> GetDataRecipientsAsync(Industry industry);
        Task<SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId);
    }
}
