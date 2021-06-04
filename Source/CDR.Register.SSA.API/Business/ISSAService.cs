using System.Threading.Tasks;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface ISSAService
    {
        Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsync(string dataRecipientBrandId, string softwareProductId);

        Task<string> GetSoftwareStatementAssertionJWTAsync(string dataRecipientBrandId, string softwareProductId);

        Task<SoftwareStatementAssertionV2Model> GetSoftwareStatementAssertionV2Async(string dataRecipientBrandId, string softwareProductId);

        Task<string> GetSoftwareStatementAssertionJWTV2Async(string dataRecipientBrandId, string softwareProductId);
    }
}
