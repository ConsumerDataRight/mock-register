using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    // Put all tests in same collection because we need them to run sequentially since some tests are mutating DB.
    [Collection("IntegrationTests")]
    [TestCaseOrderer("CDR.Register.IntegrationTests.XUnit.Orderers.AlphabeticalOrderer", "CDR.Register.IntegrationTests")]
    [DisplayTestMethodName]
    public abstract class BaseTest0
    {
    }
}
