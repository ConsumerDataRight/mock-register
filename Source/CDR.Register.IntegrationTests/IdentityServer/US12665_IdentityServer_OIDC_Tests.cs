using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CDR.Register.IntegrationTests.IdentityServer
{
    /// <summary>
    /// Integration tests for Identity Server OIDC.
    /// </summary>   
    public class US12665_IdentityServer_OIDC_Tests : BaseTest
    {
        [Fact]
        public async Task AC01_Get_ShouldRespondWith_200OK_OIDC()
        {
            // Arrange

            // Act
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/idp/.well-known/openid-configuration",
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check json
                string expected = $@"
                    {{
                    ""issuer"": ""{TLS_BaseURL}/idp"",
                    ""jwks_uri"": ""{TLS_BaseURL}/idp/.well-known/openid-configuration/jwks"",
                    ""token_endpoint"": ""{MTLS_BaseURL}/idp/connect/token"",
                    ""claims_supported"": [""sub""],
                    ""id_token_signing_alg_values_supported"": [""PS256""],
                    ""subject_types_supported"": [""public""],
                    ""scopes_supported"": [""cdr-register:bank:read"",""cdr-register:read""],
                    ""response_types_supported"": [""token""],
                    ""grant_types_supported"": [""client_credentials""],
                    ""token_endpoint_auth_methods_supported"": [""private_key_jwt""],
                    ""tls_client_certificate_bound_access_tokens"": true,
                    ""token_endpoint_auth_signing_alg_values_supported"": [""PS256""]
                    }}";

                await Assert_HasContent_Json(expected, response.Content);
            }
        }
    }
}
