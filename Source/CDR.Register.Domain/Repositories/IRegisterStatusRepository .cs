using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Domain.Repositories
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Industry industry);
        Task<SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Industry industry);
    }

}
