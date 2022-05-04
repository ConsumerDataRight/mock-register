using System.Threading.Tasks;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface ITokenizerService
    {
        Task<string> GenerateJwtTokenAsync<T>(T ssa)
            where T : SoftwareStatementAssertionModel;
    }
}
