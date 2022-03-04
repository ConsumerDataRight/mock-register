using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Infrastructure;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatusV1[]> GetDataRecipientStatusesAsyncV1();
        Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(IndustryEnum industry);
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsyncV1();
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(IndustryEnum industry);
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync();
    }
}