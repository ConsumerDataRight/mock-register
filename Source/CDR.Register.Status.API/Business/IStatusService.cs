using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.Status.API.Business
{
    public interface IStatusService
    {
        Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(Industry industry);
        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(Industry industry);
    }
}
