using CDR.Register.Domain.Entities;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        SoftwareStatementAssertionModel Map(SoftwareStatementAssertion softwareStatementAssertion);
        SoftwareStatementAssertionModelV2 MapV2(SoftwareStatementAssertion softwareStatementAssertion);
        SoftwareStatementAssertionModelV3 MapV3(SoftwareStatementAssertion softwareStatementAssertion);
    }
}
