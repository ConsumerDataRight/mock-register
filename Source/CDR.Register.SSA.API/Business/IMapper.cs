using CDR.Register.Domain.Entities;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        SoftwareStatementAssertionModel MapV3(SoftwareStatementAssertion softwareStatementAssertion);
    }
}
