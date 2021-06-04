using CDR.Register.Domain.Entities;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        public ResponseRegisterDataRecipientStatusList Map(DataRecipientStatus[] dataRecipientStatuses);
        public ResponseRegisterSoftwareProductStatusList Map(SoftwareProductStatus[] softwareProductStatuses);
    }
}
