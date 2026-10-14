using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;
using Xunit.v3;

namespace CDR.Register.IntegrationTests.XUnit.Orderers
{
    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        IReadOnlyCollection<TTestCase> ITestCaseOrderer.OrderTestCases<TTestCase>(
            IReadOnlyCollection<TTestCase> testCases)
        {
            return testCases.OrderBy(tc => ExtractTestName(tc)).ToList().AsReadOnly();
        }

        private static string ExtractTestName(ITestCase testCase)
        {
            var testMethod = testCase?.TestMethod;
            if (testMethod is ITestMethodMetadata testMethodMetadata)
            {
                return testMethodMetadata.MethodName ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
