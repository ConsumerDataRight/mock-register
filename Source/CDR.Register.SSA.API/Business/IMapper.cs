using CDR.Register.Domain.Entities;
using CDR.Register.SSA.API.Business.Models;

namespace CDR.Register.SSA.API.Business
{
    public interface IMapper
    {
        SoftwareStatementAssertionModel Map(SoftwareStatementAssertion softwareStatementAssertion);

        SoftwareStatementAssertionV2Model MapV2(SoftwareStatementAssertion softwareStatementAssertion);
    }
}
