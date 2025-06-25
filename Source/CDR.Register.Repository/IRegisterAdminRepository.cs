using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;

namespace CDR.Register.Repository
{
    public interface IRegisterAdminRepository
    {
        Task<DataHolderBrand> GetDataHolderBrandAsync(Guid brandId);

        public Task<BusinessRuleError> AddOrUpdateDataRecipient(DataRecipient dataRecipient);

        Task<bool> SaveDataHolderBrand(Guid legalEntityId, DataHolderBrand dataHolderBrand);
    }
}
