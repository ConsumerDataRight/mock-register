using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace CDR.Register.IntegrationTests.IdentityServer
{
    /// <summary>
    /// Integration tests for Identity Server OIDC.
    /// </summary>   
    public class US12665_IdentityServer_OIDC_Tests : BaseTest
    {
        public US12665_IdentityServer_OIDC_Tests(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }
        [Fact]
        public async Task AC01_Get_ShouldRespondWith_200OK_OIDC()
        {

            // Act
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/idp/.well-known/openid-configuration",
            }.SendAsync();

            // Assert
            await AssertOidcConfiguration(response, TLS_BaseURL, MTLS_BaseURL);

        }

        [Trait("Category", "CTSONLY")]
        [Fact]
        public async Task AC02_Get_ShouldRespondWith_200OK_OIDC()
        {
            // Arrange
            string ctsBaseUrl = $"{GenerateDynamicCtsUrl(IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL)}";
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{ctsBaseUrl}/idp/.well-known/openid-configuration",
            }.SendAsync();

            // Assert
            await AssertOidcConfiguration(response, ReplacePublicHostName(ctsBaseUrl, IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL), ReplaceSecureHostName(ctsBaseUrl, IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL));

        }

        private static async Task AssertOidcConfiguration(HttpResponseMessage response, string publicBaseUrl, string secureBaseUrl)
        {
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check json
                string expected = $@"
                    {{
                    ""issuer"": ""{publicBaseUrl}/idp"",
                    ""jwks_uri"": ""{publicBaseUrl}/idp/.well-known/openid-configuration/jwks"",
                    ""token_endpoint"": ""{secureBaseUrl}/idp/connect/token"",
                    ""claims_supported"": [""sub""],
                    ""id_token_signing_alg_values_supported"": [""PS256""],
                    ""subject_types_supported"": [""public""],
                    ""scopes_supported"": [""cdr-register:read""],
                    ""code_challenge_methods_supported"": [""plain"",""S256""],
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
