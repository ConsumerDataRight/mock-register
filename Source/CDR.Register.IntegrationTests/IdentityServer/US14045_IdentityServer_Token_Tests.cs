using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using AccessToken = CDR.Register.IntegrationTests.Models.AccessToken;

#nullable enable

namespace CDR.Register.IntegrationTests.IdentityServer
{
    /// <summary>
    /// Integration tests for Identity Server Token Tests.
    /// </summary>   
    public class US14045_IdentityServer_Token_Tests : BaseTest
    {
        protected const string CLIENTASSERTION_ISSUER = "86ecb655-9eba-409c-9be3-59e7adf7080d";

        protected static string CLIENTASSERTION_AUDIENCE => MTLS_BaseURL + "/idp/connect/token";

        protected const string CLIENTASSERTION_GRANT_TYPE = "client_credentials";
        protected const string CLIENTASSERTION_CLIENT_ID = "86ecb655-9eba-409c-9be3-59e7adf7080d";
        protected const string CLIENTASSERTION_SCOPE = "cdr-register:bank:read cdr-register:read";
        protected const string CLIENTASSERTION_CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

        private static HttpClient GetClient()
        {
            var _clientHandler = new HttpClientHandler();
            _clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            return new HttpClient(_clientHandler);
        }

        private static HttpClient GetClientWithCertificate(
           string certificateFilename = CERTIFICATE_FILENAME,
           string certificatePassword = CERTIFICATE_PASSWORD)
        {
            var _clientHandler = new HttpClientHandler();
            _clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Attach certificate
            var clientCertificate = new X509Certificate2(certificateFilename, certificatePassword, X509KeyStorageFlags.Exportable);
            _clientHandler.ClientCertificates.Add(clientCertificate);

            return new HttpClient(_clientHandler);
        }

        private static string GetClientAssertion(
            string certificateFilename = CERTIFICATE_FILENAME,
            string certificatePassword = CERTIFICATE_PASSWORD)
        {
            var tokenizer = new PrivateKeyJwt(certificateFilename, certificatePassword);
            return tokenizer.Generate(CLIENTASSERTION_ISSUER, CLIENTASSERTION_AUDIENCE);
        }

        private static HttpRequestMessage GetAccessTokenRequest(
           string grant_type,
           string client_id,
           string client_assertion_type,
           string client_assertion)
        {
            static string BuildContent(string grant_type, string client_id, string client_assertion_type, string client_assertion)
            {
                var kvp = new KeyValuePairBuilder();

                kvp.Add("scope", CLIENTASSERTION_SCOPE);

                if (grant_type != null)
                {
                    kvp.Add("grant_type", grant_type);
                }

                if (client_id != null)
                {
                    kvp.Add("client_id", client_id);
                }

                if (client_assertion_type != null)
                {
                    kvp.Add("client_assertion_type", client_assertion_type);
                }

                if (client_assertion != null)
                {
                    kvp.Add("client_assertion", client_assertion);
                }

                return kvp.Value;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, IDENTITYSERVER_URL)
            {
                Content = new StringContent(
                    BuildContent(grant_type, client_id, client_assertion_type, client_assertion),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            return request;
        }
       
        [Fact]
        public async Task AC01_AC06_TokenRequest_Valid_ShouldRespondWith_200OK_AccessToken()
        {
            static async Task<SecurityToken> GetValidatedToken(AccessToken accessToken)
            {
                // Get the JWKS from the Register endpoint.
                var jwksClient = GetClient();
                var jwksResponse = await jwksClient.GetAsync($"{TLS_BaseURL}/idp/.well-known/openid-configuration/jwks");
                var jwks = new JsonWebKeySet(await jwksResponse.Content.ReadAsStringAsync());

                // Setup validation paramters                
                var validationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),

                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwks.Keys.First(),

                    ValidateIssuer = true,
                    ValidIssuer = $"{TLS_BaseURL}/idp",

                    ValidateAudience = true,
                    ValidAudience = "cdr-register",
                };

                // Validate token (throws exception if token fails to validate)
                new JwtSecurityTokenHandler().ValidateToken(accessToken.access_token, validationParameters, out var validatedToken);

                return validatedToken;
            }

            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act 
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // OK response, so content should be the access token
                var accessToken = JsonSerializer.Deserialize<AccessToken>(await response.Content.ReadAsStringAsync());

                accessToken.Should().NotBeNull();
                if (accessToken != null)
                {
                    // Assert - Check expires in
                    accessToken.expires_in.Should().Be(300);

                    // Assert - Check token type
                    accessToken.token_type.Should().Be("Bearer");

                    // Assert - Check scope
                    accessToken.scope.Should().Contain("cdr-register:bank:read");

                    // Assert - Validate access token                
                    SecurityToken? validatedToken = null;
                    Func<Task> getValidatedTokenFunc = async () => validatedToken = await GetValidatedToken(accessToken);
                    await getValidatedTokenFunc.Should().NotThrowAsync(); // If token is valid then no exceptions should be thrown 
                    validatedToken?.Should().NotBeNull();

                    // Assert - Check the cnf claim
                    if (validatedToken != null)
                    {
                        var jwt = (JwtSecurityToken)validatedToken;
                        var cnf = jwt.Claims.First(c => c.Type.Equals("cnf", StringComparison.OrdinalIgnoreCase));
                        cnf.Value.Should().Contain("x5t#S256");
                        cnf.Value.Should().Contain("715CDD04FF7332CCDA74CDF9FBED16BEBA5DD744");
                    }
                }
            }
        }
        
        [Theory]
        [InlineData(null)]  // omit granttype
        [InlineData("")]    // blank granttype
        [InlineData("foo")] // non-blank 
        public async Task AC02_TokenRequest_InvalidGrantType_ShouldRespondWith_400BadRequest_ErrorResponse(string grantType)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    grantType,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error response
                var expectedContent = @"{""error"":""unsupported_grant_type""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
        

        [Theory]
        [InlineData(null)] // omit clientid
        [InlineData("")] // blank 
        [InlineData("foo")] // not blank but invalid guid
        [InlineData("99999999-9999-9999-9999-999999999999")] // valid guid but unknown clientid
        public async Task AC03_TokenRequest_InvalidClientId_ShouldRespondWith_400BadRequest_ErrorResponse(string clientid)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    clientid,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error reponse
                var expectedContent = @"{""error"":""invalid_client""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // omit client assertion type
        [InlineData("")] // blank 
        [InlineData("foo")] // invalid
        public async Task AC04_TokenRequest_InvalidClientAssertionType_ShouldRespondWith_400BadRequest_ErrorResponse(string clientAssertionType)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    clientAssertionType,
                    clientAssertion);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error reponse
                var expectedContent = @"{""error"":""invalid_client""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // omit client assertion 
        [InlineData("")] // blank 
        [InlineData("foo")] // invalid
        public async Task AC05_TokenRequest_InvalidClientAssertion_ShouldRespondWith_400BadRequest_ErrorResponse(string clientAssertion)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error reponse
                var expectedContent = @"{""error"":""invalid_client""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(ADDITIONAL_CERTIFICATE_FILENAME, ADDITIONAL_CERTIFICATE_PASSWORD)]  // invalid certificate
        public async Task AC07_TokenRequest_ValidWithInvalidClientCertificate_ShouldRespondWith_400BadRequest_ErrorResponse(string certificateFilename, string certificatePassword)
        {
            // Arrange
            var client = GetClientWithCertificate(certificateFilename, certificatePassword);

            var clientAssertion = GetClientAssertion(certificateFilename, certificatePassword);

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error reponse
                var expectedContent = @"{""error"":""invalid_client""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
    }
}
