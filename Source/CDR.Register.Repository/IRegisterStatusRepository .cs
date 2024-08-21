using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Infrastructure;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Infrastructure.Industry industry);
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Infrastructure.Industry industry);
        Task<DataHolderStatus[]> GetDataHolderStatusesAsync(Infrastructure.Industry industry);
    }
}