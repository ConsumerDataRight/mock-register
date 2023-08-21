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
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using AccessToken = CDR.Register.IntegrationTests.Models.AccessToken;

#nullable enable

namespace CDR.Register.IntegrationTests.IdentityServer
{
    /// <summary>
    /// Integration tests for Identity Server Token Tests.
    /// </summary>   
    public class US14045_IdentityServer_Token_Tests : BaseTest
    {
        public US14045_IdentityServer_Token_Tests(ITestOutputHelper outputHelper) : base(outputHelper) { }
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
            string certificatePassword = CERTIFICATE_PASSWORD, 
            string clientAssertionIssuer = CLIENTASSERTION_ISSUER,
            string? clientAssetionAudience = null,
            string subClaim = CLIENTASSERTION_ISSUER, 
            string? jtiClaim = null, 
            string? signingAlgorithm = SecurityAlgorithms.RsaSsaPssSha256)
        {
            if (clientAssetionAudience == null)
            {
                clientAssetionAudience = CLIENTASSERTION_AUDIENCE;
            }

            if(jtiClaim == null)
            {
                jtiClaim = Guid.NewGuid().ToString();
            }

            var tokenizer = new PrivateKeyJwt(certificateFilename, certificatePassword, jtiClaim);
            return tokenizer.Generate(clientAssertionIssuer, clientAssetionAudience, subClaim, signingAlgorithm: signingAlgorithm);
        }

        private static HttpRequestMessage GetAccessTokenRequest(
           string grant_type,
           string client_id,
           string client_assertion_type,
           string client_assertion, 
           string? content_type_header = "application/x-www-form-urlencoded",
           string scope = CLIENTASSERTION_SCOPE)
        {
            static string BuildContent(string grant_type, string client_id, string client_assertion_type, string client_assertion, string scope)
            {
                var kvp = new KeyValuePairBuilder();

                kvp.Add("scope", scope);

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
                    BuildContent(grant_type, client_id, client_assertion_type, client_assertion, scope),
                    Encoding.UTF8,
                    content_type_header)
            };

            if (content_type_header == null)
            {
                request.Content.Headers.ContentType = null;
            }
            else
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(content_type_header);
            }

            return request;
        }

        [Theory]
        [InlineData(null)] // null client_id should pass
        [InlineData(CLIENTASSERTION_CLIENT_ID)] // valid client_id should pass
        public async Task AC01_AC03_TokenRequest_Valid_ShouldRespondWith_200OK_AccessToken(string clientId)
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
                    clientId,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            // Act 
            var response = await client.SendAsync(request);

            Log.Information($"Response from {IDENTITYSERVER_URL}: {response.StatusCode} \n{response.Content.ReadAsStringAsync().Result}");

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
                        cnf.Value.Should().Contain("F0E5146A51F16E236844CF0353D791F11865E405");
                    }
                }
            }
        }

        [Theory]
        [InlineData(null, HttpStatusCode.BadRequest, @"{""error"":""invalid_request"",""error_description"":""Content-Type is not application/x-www-form-urlencoded""}")]
        [InlineData("application/foo", HttpStatusCode.UnsupportedMediaType, null)]
        public async Task AC04_TokenRequest_InvaldiContentType_ShouldRespondWith_400BadRequest_ErrorResponse(string contentTypeHeader, HttpStatusCode expectedHttpStatus, string expectedError)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion,
                    content_type_header: contentTypeHeader);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedHttpStatus);

                if (expectedError != null)
                {
                    // Assert - Check error response 
                    await Assert_HasContent_Json(expectedError, response.Content);
                }
            }
        }

        [Theory]
        [InlineData(null)]  // omit granttype
        [InlineData("")]    // blank granttype
        [InlineData("foo")] // non-blank 
        public async Task AC05_TokenRequest_InvalidGrantType_ShouldRespondWith_400BadRequest_ErrorResponse(string grantType)
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
                var expectedContent = String.IsNullOrEmpty(grantType) ?
                    @"{""error"":""invalid_client"",""error_description"":""grant_type not provided""}" :
                    @"{""error"":""unsupported_grant_type"",""error_description"":""grant_type must be client_credentials""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // omit client assertion 
        [InlineData("")] // blank 
        [InlineData("foo")] // invalid
        public async Task AC06_TokenRequest_InvalidClientAssertion_ShouldRespondWith_400BadRequest_ErrorResponse(string clientAssertion)
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
                var expectedContent = String.IsNullOrEmpty(clientAssertion) ?
                    @"{""error"":""invalid_client"",""error_description"":""client_assertion not provided""}" :
                    @"{""error"":""invalid_client"",""error_description"":""Invalid client_assertion - token validation error""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // omit client assertion type
        [InlineData("")] // blank 
        [InlineData("foo")] // invalid
        public async Task AC07_AC08_TokenRequest_InvalidClientAssertionType_ShouldRespondWith_400BadRequest_ErrorResponse(string clientAssertionType)
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
                var expectedContent = String.IsNullOrEmpty(clientAssertionType) ?
                    @"{""error"":""invalid_client"",""error_description"":""client_assertion_type not provided""}" :
                    @"{""error"":""invalid_client"",""error_description"":""client_assertion_type must be urn:ietf:params:oauth:client-assertion-type:jwt-bearer""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Fact]
        public async Task AC09_TokenRequest_InvaldScope_ShouldRespondWith_400BadRequest_ErrorResponse()
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion();

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion,
                    scope:"foo");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error response -WAIT FOR ROB
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""invalid scope""}";

                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Fact]
        public async Task AC11_TokenRequest_MissingJTI_ShouldRespondWith_400BadRequest_ErrorResponse()
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion(jtiClaim:"");

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

                // Assert - Check error response -WAIT FOR ROB
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""Invalid client_assertion - 'jti' is required""}";

                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Fact]
        public async Task AC12_TokenRequest_InvalidSigningAlgo_ShouldRespondWith_400BadRequest_ErrorResponse()
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion(signingAlgorithm: SecurityAlgorithms.RsaSha512);

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

                // Assert - Check error response
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""Invalid client_assertion - Client assertion token signature algorithm must be PS256 or ES256""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData("foo", CLIENTASSERTION_ISSUER, CLIENTASSERTION_CLIENT_ID, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id")] // client_id not blank but invalid guid
        [InlineData("99999999-9999-9999-9999-999999999999", CLIENTASSERTION_ISSUER, CLIENTASSERTION_CLIENT_ID, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id")] // valid guid but unknown clientid
        [InlineData(CLIENTASSERTION_CLIENT_ID, "foo", CLIENTASSERTION_CLIENT_ID, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id")] // invalid issuer
        [InlineData(CLIENTASSERTION_CLIENT_ID, CLIENTASSERTION_ISSUER, "foo", "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id")] // invalid subject
        [InlineData(null, CLIENTASSERTION_CLIENT_ID, "foo", "Invalid client_assertion - 'sub' and 'iss' must have the same value")] // invalid issuer - null client_id scenario
        public async Task AC13_AC14_TokenRequest_InvalidClientId_ShouldRespondWith_400BadRequest_ErrorResponse(string clientid, string issuer, string subject, string expectedError)
        {
            // Arrange
            var client = GetClientWithCertificate();

            var clientAssertion = GetClientAssertion(clientAssertionIssuer: issuer, subClaim: subject);

            Log.Information($"Client Assertion used: {clientAssertion}");

            var request = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    clientid,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);

            if (request.Content != null)
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                Log.Information($"Token Request content: {requestContent}");
            }
            else
            {
                Log.Information($"Token content is null.");
            }

            // Act
            var response = await client.SendAsync(request);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error reponse
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""" + expectedError + @"""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Fact]
        public async Task AC15_TokenRequest_DuplicateJTI_ShouldRespondWith_400BadRequest_ErrorResponse()
        {
            // Arrange
            var client = GetClientWithCertificate();

            string jti = Guid.NewGuid().ToString();

            var clientAssertion = GetClientAssertion(jtiClaim: jti);

            var firstRequest = GetAccessTokenRequest(
                    CLIENTASSERTION_GRANT_TYPE,
                    CLIENTASSERTION_CLIENT_ID,
                    CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                    clientAssertion);
            
            _ = await client.SendAsync(firstRequest);

            // Act
            // Create new Access Token request with the same (duplicate) JTI
            var secondRequest = GetAccessTokenRequest(
                CLIENTASSERTION_GRANT_TYPE,
                CLIENTASSERTION_CLIENT_ID,
                CLIENTASSERTION_CLIENT_ASSERTION_TYPE,
                clientAssertion);

            HttpResponseMessage? response = await client.SendAsync(secondRequest);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error response 
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""Invalid client_assertion - 'jti' in the client assertion token must be unique""}";

                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
    
        [Theory]
        [InlineData(ADDITIONAL_CERTIFICATE_FILENAME, ADDITIONAL_CERTIFICATE_PASSWORD)]  // invalid certificate
        public async Task AC16_TokenRequest_ValidWithInvalidClientCertificate_ShouldRespondWith_400BadRequest_ErrorResponse(string certificateFilename, string certificatePassword)
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
                var expectedContent = @"{""error"":""invalid_client"",""error_description"":""Invalid client_assertion - token validation error""}";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
       
    }
}
