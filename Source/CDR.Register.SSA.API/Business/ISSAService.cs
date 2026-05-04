using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface ISsaService
    {
        Task<string> GetSoftwareStatementAssertionJWTAsync(string dataRecipientBrandId, string softwareProductId);
    }
}
