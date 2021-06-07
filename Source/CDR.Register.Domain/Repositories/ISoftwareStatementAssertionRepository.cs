using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Domain.Repositories
{
    public interface ISoftwareStatementAssertionRepository
    {
        Task<SoftwareStatementAssertion> GetSoftwareStatementAssertionAsync(Guid dataRecipientBrandId, Guid softwareProductId);
    }
}
