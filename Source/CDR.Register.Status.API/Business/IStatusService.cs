using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.Status.API.Business
{
    public interface IStatusService
    {
        Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatuses(Industry industry, int version);

        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatuses(Industry industry, int version);

        Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatuses(Industry industry, int version);
    }
}
