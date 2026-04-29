using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Specifications;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterDiscoveryRepository
    {
        Task<Page<DataHolderBrand[]>> GetDataHolderBrands(Infrastructure.Industry industry, IBrandSpecification specification, DateTime? updatedSince, int page, int pageSize);

        Task<DataRecipient[]> GetDataRecipientsAsync();

        Task<SoftwareProduct> GetSoftwareProductId(Guid softwareProductId);
    }
}
