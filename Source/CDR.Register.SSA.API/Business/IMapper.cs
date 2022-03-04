using CDR.Register.Domain.Entities;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        SoftwareStatementAssertionModelV2 MapV2(SoftwareStatementAssertion softwareStatementAssertion);

        SoftwareStatementAssertionModel Map(SoftwareStatementAssertion softwareStatementAssertion);
    }
}
