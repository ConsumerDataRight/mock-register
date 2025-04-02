using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface ISsaService
    {
        Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsync(Industry industry, string dataRecipientBrandId, string softwareProductId);

        Task<string> GetSoftwareStatementAssertionJWTAsync(Industry industry, string dataRecipientBrandId, string softwareProductId);
    }
}
