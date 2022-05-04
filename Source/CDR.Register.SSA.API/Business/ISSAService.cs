using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface ISSAService
    {
        Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsyncXV1(Industry industry, string dataRecipientBrandId, string softwareProductId);
        Task<SoftwareStatementAssertionModelV2> GetSoftwareStatementAssertionAsyncXV2(Industry industry, string dataRecipientBrandId, string softwareProductId);
        Task<SoftwareStatementAssertionModelV3> GetSoftwareStatementAssertionAsyncXV3(Industry industry, string dataRecipientBrandId, string softwareProductId);

        Task<string> GetSoftwareStatementAssertionJWTAsyncXV1(Industry industry, string dataRecipientBrandId, string softwareProductId);
        Task<string> GetSoftwareStatementAssertionJWTAsyncXV2(Industry industry, string dataRecipientBrandId, string softwareProductId);
        Task<string> GetSoftwareStatementAssertionJWTAsyncXV3(Industry industry, string dataRecipientBrandId, string softwareProductId);
    }
}
