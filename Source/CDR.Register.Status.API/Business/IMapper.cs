using CDR.Register.Domain.Entities;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        public ResponseRegisterDataRecipientStatusListV1 Map(DataRecipientStatusV1[] dataRecipientStatuses);
        public ResponseRegisterDataRecipientStatusList Map(DataRecipientStatus[] dataRecipientStatuses);
        public ResponseRegisterSoftwareProductStatusListV1 Map(SoftwareProductStatus[] softwareProductStatuses);
    }
}
