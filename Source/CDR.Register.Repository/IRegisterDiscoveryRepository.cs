using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterDiscoveryRepository
    {
        Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsync(Infrastructure.Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<DataRecipient[]> GetDataRecipientsAsync(Infrastructure.Industry industry);
        Task<SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId);
    }
}
