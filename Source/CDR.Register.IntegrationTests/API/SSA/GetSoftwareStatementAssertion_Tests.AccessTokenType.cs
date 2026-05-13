#nullable enable

namespace CDR.Register.IntegrationTests.API.SSA
{
public partial class GetSoftwareStatementAssertion_Tests
    {
        private enum AccessTokenType
        {
            ValidAccessToken,   // Get and send valid access token
            InvalidAccessToken, // Send an invalid access token
            ExpiredAccessToken, // Send expired access token
            NoAccessToken,      // Don't send any access token
        }
    }
}
