using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Specifications;

namespace CDR.Register.Repository.Interfaces
{
    public interface IRegisterStatusRepository
    {
        Task<DataRecipientStatus[]> GetDataRecipientStatuses(Infrastructure.Industry industry, IParticipationSpecification specification);

        Task<SoftwareProductStatus[]> GetSoftwareProductStatuses(Infrastructure.Industry industry, IParticipationSpecification specification);

        Task<DataHolderStatus[]> GetDataHolderStatuses(Infrastructure.Industry industry, IParticipationSpecification specification);
    }
}
