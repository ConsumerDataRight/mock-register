using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Domain.Repositories
{
    public interface IRegisterInfosecRepository
    {
        Task<SoftwareProductInfosec> GetSoftwareProductAsync(Guid id);
    }
}
