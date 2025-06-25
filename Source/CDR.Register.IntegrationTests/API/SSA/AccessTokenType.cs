#nullable enable

namespace CDR.Register.IntegrationTests.API.SSA
{
public partial class US27564_GetSoftwareStatementAssertion_MultiIndustry_Tests
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
