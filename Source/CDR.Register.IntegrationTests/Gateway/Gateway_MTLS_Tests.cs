using System;
using System.IdentityModel.Tokens.Jwt;
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
using Xunit;

namespace CDR.Register.IntegrationTests.Gateway
{
    /// <summary>
    /// Integration tests for mTLS Gateway.
    /// </summary>       
    public class Gateway_MTLS_Tests : BaseTest
    {
        // Client certificates
        const string INVALID_CERTIFICATE_FILENAME = "certificates\\client-invalid.pfx";

        // Server certificate
        const string SERVER_CERTIFICATE_FILENAME = "certificates\\server.pfx";
        const string SERVER_CERTIFICATE_PASSWORD = "#M0ckDataHolder#";

        // Client assertion
        private const string AUDIENCE = IDENTITYSERVER_URL;
        private const string SCOPE = "cdr-register:bank:read";

        // Token request
        private const string GRANT_TYPE = "client_credentials";
        private const string CLIENT_ID = "86ecb655-9eba-409c-9be3-59e7adf7080d";
        private const string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        private const string ISSUER = CLIENT_ID;

        // Participation/Brand/SoftwareProduct Ids
        // private const string PARTICIPATIONID = "CDF81DC4-1282-405F-BAFC-C340B788D52A";
        private string PARTICIPATIONID => GetParticipationId(BRANDID); // lookup 
        private const string BRANDID = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SOFTWAREPRODUCTID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        /// <summary>
        /// Get HttpClient with client certificate
        /// </summary>
        private static HttpClient GetClient(string certificateFilename, string certificatePassword)
        {
            var _clientHandler = new HttpClientHandler();
            _clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Attach certificate
            var clientCertificate = new X509Certificate2(certificateFilename, certificatePassword, X509KeyStorageFlags.Exportable);
            _clientHandler.ClientCertificates.Add(clientCertificate);

            return new HttpClient(_clientHandler);
        }

        /// <summary>
        /// Get HttpRequestMessage for access token request
        /// </summary>
        private static HttpRequestMessage GetAccessTokenRequest(
           string certificateFilename,
           string certificatePassword,
           string issuer = ISSUER,
           string audience = AUDIENCE,
           string scope = SCOPE,
           string grant_type = GRANT_TYPE,
           string client_id = CLIENT_ID,
           string client_assertion_type = CLIENT_ASSERTION_TYPE)
        {
            static string BuildContent(string scope, string grant_type, string client_id, string client_assertion_type, string client_assertion)
            {
                var kvp = new KeyValuePairBuilder();

                if (scope != null)
                {
                    kvp.Add("scope", scope);
                }

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

            var tokenizer = new PrivateKeyJwt(certificateFilename, certificatePassword);
            var client_assertion = tokenizer.Generate(issuer, audience);

            var request = new HttpRequestMessage(HttpMethod.Post, IDENTITYSERVER_URL)
            {
                Content = new StringContent(
                    BuildContent(scope, grant_type, client_id, client_assertion_type, client_assertion),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            return request;
        }

        [Fact]
        public async Task PostAccessTokenRequest_WithClientCert_ShouldRespondWith_200OK_AccessToken()
        {
            // Expected JWT claims           
            string JWT_CLAIM_ISS = $"{TLS_BaseURL}/idp";
            const string JWT_CLAIM_AUD = "cdr-register";
            const string JWT_CLAIM_CLIENT_ID = CLIENT_ID;
            const string JWT_CLAIM_SCOPE = SCOPE;
            const string JWT_CLAIM_CNF = @"{""x5t#S256"":""58D76F7A61CD726DA1C54F6898E8E69EA4C88060""}";

            // Expected AccessToken
            const int ACCESSTOKEN_EXPIRESIN = 300;
            const string ACCESSTOKEN_TOKENTYPE = "Bearer";
            const string ACCESSTOKEN_SCOPE = "cdr-register:bank:read";

            // Arrange 
            var client = GetClient(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);
            var accessTokenRequest = GetAccessTokenRequest(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);

            // Act
            var accessTokenResponse = await client.SendAsync(accessTokenRequest);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                accessTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                accessTokenResponse.Content.Headers.ContentType.ToString().Should().StartWith("application/json");

                // Get access token payload
                var accessToken = JsonSerializer.Deserialize<Models.AccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

                // Assert - Check expires_in
                accessToken.expires_in.Should().Be(ACCESSTOKEN_EXPIRESIN);

                // Assert - Check token_type
                accessToken.token_type.Should().Be(ACCESSTOKEN_TOKENTYPE);

                // Assert - Check scope
                accessToken.scope.Should().Be(ACCESSTOKEN_SCOPE);

                // Assert - Check the JWT access_token
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken.access_token);
                AssertClaim(jwt.Claims, "iss", JWT_CLAIM_ISS);
                AssertClaim(jwt.Claims, "aud", JWT_CLAIM_AUD);
                AssertClaim(jwt.Claims, "client_id", JWT_CLAIM_CLIENT_ID);
                AssertClaim(jwt.Claims, "scope", JWT_CLAIM_SCOPE);
                AssertClaim(jwt.Claims, "cnf", JWT_CLAIM_CNF);
            }
        }

        [Fact]
        public async Task PostAccessTokenRequest_WithClientCertNotAssociatedToClient_ShouldRespondWith_400BadRequest()
        {
            // Arrange
            var client = GetClient(ADDITIONAL_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);
            var accessTokenRequest = GetAccessTokenRequest(ADDITIONAL_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);

            // Act
            var accessTokenResponse = await client.SendAsync(accessTokenRequest);

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                accessTokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error response
                var expectedContent = @"{ ""error"": ""invalid_client"" }";
                await Assert_HasContent_Json(expectedContent, accessTokenResponse.Content);
            }
        }

        [Fact]
        public async Task PostAccessTokenRequest_WithExpiredClientCert_ShouldThrow_HttpRequestException()
        {
            // Arrange
            var client = GetClient(INVALID_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);
            var accessTokenRequest = GetAccessTokenRequest(INVALID_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD);

            // Act/Assert
            Func<Task> act = async () => await client.SendAsync(accessTokenRequest);
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<HttpRequestException>();
            }
        }

        [Fact]
        public async Task PostAccessTokenRequest_WithServerCert_ShouldThrow_HttpRequestException()
        {
            // Arrange
            var client = GetClient(SERVER_CERTIFICATE_FILENAME, SERVER_CERTIFICATE_PASSWORD);
            var accessTokenRequest = GetAccessTokenRequest(SERVER_CERTIFICATE_FILENAME, SERVER_CERTIFICATE_PASSWORD);

            // Act/Assert
            Func<Task> act = async () => await client.SendAsync(accessTokenRequest);
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<HttpRequestException>();
            }
        }


        delegate void BeforeDataHolderBrandsRequest();
        delegate void AfterDataHolderBrandsRequest();
        private static async Task Test_GetDataHolderBrands(
            string certificateFilename,
            string certificatePassword,
            HttpStatusCode expectedStatusCode,
            bool withAccessToken = true,
            BeforeDataHolderBrandsRequest beforeRequest = null,
            AfterDataHolderBrandsRequest afterRequest = null)
        {
            // DataHolderBrands
            string URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands";
            const string XV = "1";

            // Arrange
            var client = GetClient(certificateFilename, certificatePassword);

            var request = new HttpRequestMessage(HttpMethod.Get, URL);
            request.Headers.Add("x-v", XV);

            // Supply access token with request?
            if (withAccessToken)
            {
                var accessTokenRequest = GetAccessTokenRequest(certificateFilename, certificatePassword);
                var accessTokenResponse = await client.SendAsync(accessTokenRequest);
                var accessToken = JsonSerializer.Deserialize<Models.AccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.access_token);
            }

            beforeRequest?.Invoke();
            try
            {
                // Act
                var response = await client.SendAsync(request);

                // Assert
                using (new AssertionScope())
                {
                    // Assert - Check status code
                    response.StatusCode.Should().Be(expectedStatusCode);
                }
            }
            finally
            {
                afterRequest?.Invoke();
            }
        }

        [Fact]
        public async Task GetDataHolderBrands_WithAccessToken_AndClientCert_ShouldRespondWith_200OK()
        {
            await Test_GetDataHolderBrands(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetDataHolderBrands_WithAccessToken_AndExpiredClientCert_ShouldThrow_HttpRequestException()
        {
            Func<Task> test = async () => await Test_GetDataHolderBrands(INVALID_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
            await test.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetDataHolderBrands_WithAccessToken_AndClientCertNotAssociatedToAccessToken_ShouldRespondWith_401Unauthorised()
        {
            await Test_GetDataHolderBrands(ADDITIONAL_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetDataHolderBrands_WithAccessToken_AndServerCertNotAssociatedToAccessToken_ShouldThrow_HttpRequestException()
        {
            Func<Task> test = async () => await Test_GetDataHolderBrands(SERVER_CERTIFICATE_FILENAME, SERVER_CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
            await test.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetDataHolderBrands_WithNoAccessToken_AndClientCert_ShouldRespondWith_401Unauthorised()
        {
            await Test_GetDataHolderBrands(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized, false);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed
        public async Task GetDataHolderBrands_WithAccessToken_AndSoftwareProductStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Test_GetDataHolderBrands(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId));
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task GetDataHolderBrands_WithAccessToken_AndBrandStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Test_GetDataHolderBrands(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId)
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden)] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden)] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden)] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden)] // Inactive 
        public async Task GetDataHolderBrands_WithAccessToken_AndParticipationStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Test_GetDataHolderBrands(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId)
            );
        }

        delegate void BeforeSSARequest();
        delegate void AfterSSARequest();
        private static async Task Test_GetSSA(
            string certificateFilename,
            string certificatePassword,
            HttpStatusCode expectedStatusCode,
            bool withAccessToken = true,
            BeforeSSARequest beforeRequest = null,
            AfterSSARequest afterRequest = null)
        {      
            string URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/{BRANDID}/software-products/{SOFTWAREPRODUCTID}/ssa";
            const string XV = "1";

            // Arrange
            var client = GetClient(certificateFilename, certificatePassword);

            var request = new HttpRequestMessage(HttpMethod.Get, URL);
            request.Headers.Add("x-v", XV);

            // Supply access token with request?
            if (withAccessToken)
            {
                var accessTokenRequest = GetAccessTokenRequest(certificateFilename, certificatePassword);
                var accessTokenResponse = await client.SendAsync(accessTokenRequest);
                var accessToken = JsonSerializer.Deserialize<Models.AccessToken>(await accessTokenResponse.Content.ReadAsStringAsync());

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.access_token);
            }

            beforeRequest?.Invoke();
            try
            {
                // Act
                var response = await client.SendAsync(request);

                // Assert
                using (new AssertionScope())
                {
                    // Assert - Check status code
                    response.StatusCode.Should().Be(expectedStatusCode);
                }
            }
            finally
            {
                afterRequest?.Invoke();
            }
        }

        [Fact]
        public async Task GetSSA_WithAccessToken_AndClientCert_ShouldRespondWith_200OK()
        {
            await Test_GetSSA(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetSSA_WithAccessToken_AndExpiredClientCert_ShouldThrow_HttpRequestException()
        {
            Func<Task> test = async () => await Test_GetSSA(INVALID_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
            await test.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetSSA_WithAccessToken_AndClientCertNotAssociatedToAccessToken_ShouldRespondWith_401Unauthorised()
        {
            await Test_GetSSA(ADDITIONAL_CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSSA_WithAccessToken_AndServerCertNotAssociatedToAccessToken_ShouldThrow_HttpRequestException()
        {
            Func<Task> test = async () => await Test_GetSSA(SERVER_CERTIFICATE_FILENAME, SERVER_CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized);
            await test.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task GetSSA_WithNoAccessToken_AndClientCert_ShouldRespondWith_401Unauthorised()
        {
            await Test_GetSSA(CERTIFICATE_FILENAME, CERTIFICATE_PASSWORD, HttpStatusCode.Unauthorized, false);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task GetSSA_WithAccessToken_AndSoftwareProductStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId)
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task GetSSA_WithAccessToken_AndBrandStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId)
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden)] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden)] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden)] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden)] // Inactive 
        public async Task GetSSA_WithAccessToken_AndParticipationStatusSetInactiveSinceTokenProvisioned_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId)
            );
        }
    }
}
