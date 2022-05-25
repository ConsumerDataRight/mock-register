using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Infrastructure;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Industry industry);
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Industry industry);
        Task<DataHolderStatus[]> GetDataHolderStatusesAsync(Industry industry);
    }
}