using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Infrastructure.Industry industry);

        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Infrastructure.Industry industry);

        Task<DataHolderStatus[]> GetDataHolderStatusesAsync(Infrastructure.Industry industry);
    }
}
