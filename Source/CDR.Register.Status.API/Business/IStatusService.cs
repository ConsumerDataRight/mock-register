using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business.Responses;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Business
{
    public interface IStatusService
    {
        Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsyncXV1(Industry industry);
        Task<ResponseRegisterDataRecipientStatusListV2> GetDataRecipientStatusesAsyncXV2(Industry industry);
        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsyncXV1(Industry industry);
        Task<ResponseRegisterSoftwareProductStatusListV2> GetSoftwareProductStatusesAsyncXV2(Industry industry);
        Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatusesAsyncXV1(Industry industry);
    }
}