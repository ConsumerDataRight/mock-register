using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Microsoft.Extensions.Configuration;

#nullable enable

namespace CDR.Register.IntegrationTests.API.SSA
{
    /// <summary>
    /// Integration tests for GetSoftwareStatementAssertion.
    /// </summary>   
    public class US27564_GetSoftwareStatementAssertion_MultiIndustry_Tests : BaseTest
    {
        // Participation/Brand/SoftwareProduct Ids
        private static string PARTICIPATIONID => GetParticipationId(BRANDID); // lookup 
        private const string BRANDID = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SOFTWAREPRODUCTID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        enum AccessTokenType
        {
            ValidAccessToken,   // Get and send valid access token
            InvalidAccessToken, // Send an invalid access token
            ExpiredAccessToken, // Send expired access token
            NoAccessToken,      // Don't send any access token
        };
        delegate void BeforeSSARequest();
        delegate void AfterSSARequest();

        private static async Task Test_GetSSA(
           string certificateFilename,
           string certificatePassword,
           HttpStatusCode expectedStatusCode,
           AccessTokenType accessTokenType = AccessTokenType.ValidAccessToken,
           BeforeSSARequest? beforeRequest = null,
           AfterSSARequest? afterRequest = null,
           string? x_v = "3",
           string? x_min_v = null,
           string? expectedContent = null,
           string? getAccessTokenCertificateFilename = null,
           string? getAccessTokenCertificatePassword = null,
           string? expectedXV = null,
           string? industry = "all",
           string brandId = BRANDID,
           string softwareProductId = SOFTWAREPRODUCTID)
        {
            async Task<string?> GetAccessToken()
            {
                // Access token
                switch (accessTokenType)
                {
                    case AccessTokenType.ValidAccessToken:
                        // Get the access token with the valid certificate.
                        return await new Infrastructure.AccessToken
                        {
                            CertificateFilename = getAccessTokenCertificateFilename ?? certificateFilename,
                            CertificatePassword = getAccessTokenCertificatePassword ?? certificatePassword
                        }.GetAsync();

                    case AccessTokenType.InvalidAccessToken:
                        return "foo";

                    case AccessTokenType.ExpiredAccessToken:
                        // Represents an expired access token.
                        // "exp": 1621344825
                        // Expired at "Tuesday, May 18, 2021 11:33:45 PM GMT+10:00"
                        return "eyJhbGciOiJQUzI1NiIsImtpZCI6IkFBMjRGMTg1RUUzRjY3NTA0ODA4RkM0RTI2QjEzNUI5OUU2M0JEQTkiLCJ0eXAiOiJhdCtqd3QiLCJ4NXQiOiJxaVR4aGU0X1oxQklDUHhPSnJFMXVaNWp2YWsifQ.eyJuYmYiOjE2MjEzNDQ1MjUsImV4cCI6MTYyMTM0NDgyNSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzAwMC9pZHAiLCJhdWQiOiJjZHItcmVnaXN0ZXIiLCJjbGllbnRfaWQiOiI2ZjdhMWI4ZS04Nzk5LTQ4YTgtOTAxMS1lMzkyMDM5MWY3MTMiLCJqdGkiOiJDODRBNTM5MTA2QjI4NUJBODI2RjZGMDQ3MjU4RjBBNCIsImlhdCI6MTYyMTM0NDUyNSwic2NvcGUiOlsiY2RyLXJlZ2lzdGVyOmJhbms6cmVhZCJdLCJjbmYiOnsieDV0I1MyNTYiOiI1OEQ3NkY3QTYxQ0Q3MjZEQTFDNTRGNjg5OEU4RTY5RUE0Qzg4MDYwIn19.RTU-zrqkb-WXcJzCz62SJ4h19lj8MDyGcvLOmg0qx05WFbAsY4mEP3gsoqM1LJfq4ncw7RqSvbkCNQQ-NOnyoBHF8MGe7mzdUh3YrD0_lTg20Dkx1-l044svtP_CKTI3rXT3bZaYWce0Tb1s3mrJzfN3ja23o93FGR-wbIwHp2347b0DxjznpKBw5meLhAjS7OCx6_uMm1la6IziSQgqMd2WaA-od7w8J5br-Nn-QZZi7X1KGiPEKFDFNk8KrUdPc4NCH6t7f-Sbc34KNNEWfAOJkWdDrmsBaifSlWvSlS4nUnurGHYkmimA2JUuv3ZTqzCcLRamEER1ZoTcIs_PDw";

                    case AccessTokenType.NoAccessToken:
                        return null;

                    default:
                        throw new NotSupportedException();
                }
            }

            // Arrange
            string URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-recipients/brands/{brandId}/software-products/{softwareProductId}/ssa";

            var accessToken = await GetAccessToken();

            var api = new Infrastructure.API
            {
                CertificateFilename = certificateFilename,
                CertificatePassword = certificatePassword,
                HttpMethod = HttpMethod.Get,
                URL = URL,
                AccessToken = accessToken,
                XV = x_v,
                XMinV = x_min_v
            };

            beforeRequest?.Invoke();
            try
            {
                // Act
                var response = await api.SendAsync();

                // Assert
                using (new AssertionScope())
                {
                    // Assert - Check status code
                    response.StatusCode.Should().Be(expectedStatusCode);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // Assert - Check XV
                        Assert_HasHeader(expectedXV ?? x_v, response.Headers, "x-v");
                    }

                    // Assert - Check expected content
                    if (expectedContent != null)
                    {
                        await Assert_HasContent_Json(expectedContent, response.Content);
                    }
                }
            }
            finally
            {
                afterRequest?.Invoke();
            }
        }

        [Theory]
        [InlineData(3)]
        public async Task AC01_GetSSA_WithXV1_ShouldRespondWith_200OK_V3ofSSA(int XV)
        {
            // Arrange - Get SoftwareProduct
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);
            var softwareProduct = dbContext.SoftwareProducts.AsNoTracking()
                    .Include(sp => sp.Brand)
                    .Where(sp => sp.SoftwareProductId == new Guid(SOFTWAREPRODUCTID))
                    .Single();

            // Arrange - Get access token
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            // Act - Send request to SSA API
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/{BRANDID}/software-products/{SOFTWAREPRODUCTID}/ssa",
                XV = XV.ToString(),
                AccessToken = accessToken
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Get the SSA JWKS from the Register.
                    var ssaJwks = await GetSsaJwks();

                    // Assert - Check content type
                    // Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check XV
                    Assert_HasHeader(XV.ToString(), response.Headers, "x-v");

                    // Assert - SSA
                    var SSA = new JwtSecurityTokenHandler().ReadJwtToken(await response.Content.ReadAsStringAsync());

                    // Assert - SSA Header
                    SSA.Header.Alg.Should().Be("PS256");
                    SSA.Header.Kid.Should().Be(ssaJwks.Keys.First().Kid);
                    SSA.Header.Typ.Should().Be("JWT");

                    // Assert - SSA Claims
                    SSA.AssertClaim("iss", "cdr-register");

                    SSA.AssertClaim("iat", null);
                    SSA.AssertClaim("exp", null);
                    long iat = Convert.ToInt64(SSA.Claim("iat").Value);
                    long exp = Convert.ToInt64(SSA.Claim("exp").Value);
                    exp.Should().Be(iat + 600); // Check expiry is 10 minutes (ie 600 seconds)

                    SSA.AssertClaim("jti", null);
                    SSA.AssertClaim("org_id", softwareProduct.Brand.BrandId.ToString());
                    SSA.AssertClaim("org_name", softwareProduct.Brand.BrandName);
                    SSA.AssertClaim("client_name", softwareProduct.SoftwareProductName);
                    SSA.AssertClaim("client_description", softwareProduct.SoftwareProductDescription);
                    SSA.AssertClaim("client_uri", softwareProduct.ClientUri);
                    SSA.AssertClaimIsArray("redirect_uris", softwareProduct.RedirectUris.Split(" "));
                    SSA.AssertClaim("logo_uri", softwareProduct.LogoUri);
                    SSA.AssertClaim("tos_uri", softwareProduct.TosUri, true);
                    SSA.AssertClaim("policy_uri", softwareProduct.PolicyUri, true);
                    SSA.AssertClaim("jwks_uri", softwareProduct.JwksUri);
                    SSA.AssertClaim("revocation_uri", softwareProduct.RevocationUri);
                    SSA.AssertClaim("software_id", softwareProduct.SoftwareProductId.ToString());
                    SSA.AssertClaim("software_roles", "data-recipient-software-product");
                    SSA.AssertClaim("scope", softwareProduct.Scope);

                    if (XV >= 2)
                    {
                        SSA.AssertClaim("recipient_base_uri", softwareProduct.RecipientBaseUri);
                    }

                    // Assert - Validate SSA Signature
                    var validationParameters = new TokenValidationParameters()
                    {
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),

                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = ssaJwks.Keys.First(),

                        ValidateIssuer = true,
                        ValidIssuer = "cdr-register",

                        ValidateAudience = false,
                    };

                    // Validate token (throws exception if token fails to validate)
                    new JwtSecurityTokenHandler().ValidateToken(SSA.RawData, validationParameters, out var validatedToken);
                }
            }
        }

        [Theory]
        [InlineData("all")]
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task ACX99_GetSSA_WithDifferentIndustry_ShouldRespondWith_DifferentScopes(string industry)
        {
            // Arrange - Get SoftwareProduct
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);
            var softwareProduct = dbContext.SoftwareProducts.AsNoTracking()
                    .Include(sp => sp.Brand)
                    .Where(sp => sp.SoftwareProductId == new Guid(SOFTWAREPRODUCTID))
                    .Single();

            // Arrange - Get access token
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            // Act - Send request to SSA API
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-recipients/brands/{BRANDID}/software-products/{SOFTWAREPRODUCTID}/ssa",
                XV = "3",
                AccessToken = accessToken
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Get the SSA JWKS from the Register.
                    var ssaJwks = await GetSsaJwks();

                    // Assert - Check XV
                    Assert_HasHeader("3", response.Headers, "x-v");

                    // Assert - SSA
                    var SSA = new JwtSecurityTokenHandler().ReadJwtToken(await response.Content.ReadAsStringAsync());

                    // Assert - SSA Header
                    SSA.Header.Alg.Should().Be("PS256");
                    SSA.Header.Kid.Should().Be(ssaJwks.Keys.First().Kid);
                    SSA.Header.Typ.Should().Be("JWT");

                    // Assert - SSA Claims
                    SSA.AssertClaim("iss", "cdr-register");

                    SSA.AssertClaim("iat", null);
                    SSA.AssertClaim("exp", null);
                    long iat = Convert.ToInt64(SSA.Claim("iat").Value);
                    long exp = Convert.ToInt64(SSA.Claim("exp").Value);
                    exp.Should().Be(iat + 600); // Check expiry is 10 minutes (ie 600 seconds)

                    SSA.AssertClaim("jti", null);
                    SSA.AssertClaim("org_id", softwareProduct.Brand.BrandId.ToString());
                    SSA.AssertClaim("org_name", softwareProduct.Brand.BrandName);
                    SSA.AssertClaim("client_name", softwareProduct.SoftwareProductName);
                    SSA.AssertClaim("client_description", softwareProduct.SoftwareProductDescription);
                    SSA.AssertClaim("client_uri", softwareProduct.ClientUri);
                    SSA.AssertClaimIsArray("redirect_uris", softwareProduct.RedirectUris.Split(" "));                    
                    SSA.AssertClaim("logo_uri", softwareProduct.LogoUri);
                    SSA.AssertClaim("tos_uri", softwareProduct.TosUri, true);
                    SSA.AssertClaim("policy_uri", softwareProduct.PolicyUri, true);
                    SSA.AssertClaim("jwks_uri", softwareProduct.JwksUri);
                    SSA.AssertClaim("revocation_uri", softwareProduct.RevocationUri);
                    SSA.AssertClaim("software_id", softwareProduct.SoftwareProductId.ToString());
                    SSA.AssertClaim("software_roles", "data-recipient-software-product");
                    SSA.AssertClaim("scope", softwareProduct.Scope);
                    SSA.AssertClaim("sector_identifier_uri", softwareProduct.SectorIdentifierUri, true);

                    // Assert - Validate SSA Signature
                    var validationParameters = new TokenValidationParameters()
                    {
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),

                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = ssaJwks.Keys.First(),

                        ValidateIssuer = true,
                        ValidIssuer = "cdr-register",

                        ValidateAudience = false,
                    };

                    // Validate token (throws exception if token fails to validate)
                    new JwtSecurityTokenHandler().ValidateToken(SSA.RawData, validationParameters, out var validatedToken);
                }
            }
        }

        //[Theory]
        //[InlineData(null)] // DF: this will no longer error.
        //public async Task AC02_GetSSA_WithMissingXV_ShouldRespondWith_400BadRequest(string? XV)
        //{
        //    await Test_GetSSA(
        //        CERTIFICATE_FILENAME,
        //        CERTIFICATE_PASSWORD,
        //        HttpStatusCode.BadRequest,
        //        x_v: XV,
        //        expectedContent: @"
        //        {
        //            ""errors"": [
        //                    {
        //                    ""code"": ""urn:au-cds:error:cds-all:Header/Missing"",
        //                    ""title"": ""Missing Required Header"",
        //                    ""detail"": """",
        //                    ""meta"": {}
        //                    }
        //                ]
        //        }");
        //}

        [Theory]
        //[InlineData("")] // DF: will no longer error
        [InlineData("foo")]
        public async Task ACX02_GetSSA_WithInvalidXV_ShouldRespondWith_400BadRequest(string? XV)
        {
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.BadRequest,
                x_v: XV,
                expectedContent: @"
                {
                    ""errors"": [
                            {
                            ""code"": ""urn:au-cds:error:cds-all:Header/InvalidVersion"",
                            ""title"": ""Invalid Version"",
                            ""detail"": """",
                            ""meta"": {}
                            }
                        ]
                }");
        }

        const string EXPECTEDCONTENT_ADRSTATUSNOTACTIVE = @"
            {
                ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive"",
                        ""title"": ""ADR Status Is Not Active"",
                        ""detail"": """",
                        ""meta"": {}
                        }
                    ]
            }";

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden)] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden)] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden)] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden)] // Inactive         
        public async Task AC03_GetSSA_WithParticipationStatusNotActive_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId),
                expectedContent: expectedStatusCode == HttpStatusCode.OK ? null : EXPECTEDCONTENT_ADRSTATUSNOTACTIVE
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task AC04_GetSSA_WithBrandNotActive_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId),
                expectedContent: expectedStatusCode == HttpStatusCode.OK ? null : EXPECTEDCONTENT_ADRSTATUSNOTACTIVE
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task AC05_GetSSA_WithSoftwareProductNotActive_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId),
                expectedContent: expectedStatusCode == HttpStatusCode.OK ? null : EXPECTEDCONTENT_ADRSTATUSNOTACTIVE
            );
        }

        [Theory]
        [InlineData("3")]
        public async Task AC06_GetSSA_WithNoAccessToken_ShouldRespondWith_401Unauthorized(string x_v)
        {
            await Test_GetSSA(
                           CERTIFICATE_FILENAME,
                           CERTIFICATE_PASSWORD,
                           HttpStatusCode.Unauthorized,
                           AccessTokenType.NoAccessToken,
                           x_v: x_v);
        }

        [Theory]
        [InlineData("3")]
        public async Task AC07_GetSSA_WithInvalidAccessToken_ShouldRespondWith_401Unauthorized(string x_v)
        {
            await Test_GetSSA(
                           CERTIFICATE_FILENAME,
                           CERTIFICATE_PASSWORD,
                           HttpStatusCode.Unauthorized,
                           AccessTokenType.InvalidAccessToken,
                           x_v: x_v);
        }

        [Theory]
        [InlineData("3")]
        public async Task AC08_GetSSA_WithExpiredAccessToken_ShouldRespondWith_401Unauthorized(string x_v)
        {
            await Test_GetSSA(
                            CERTIFICATE_FILENAME,
                            CERTIFICATE_PASSWORD,
                            HttpStatusCode.Unauthorized,
                            AccessTokenType.ExpiredAccessToken,
                            x_v: x_v);
        }

        [Fact]
        public async Task AC09_GetSSA_DifferentHolderOfKey_ShouldRespondWith_401Unauthorized()
        {
            await Test_GetSSA(
                            ADDITIONAL_CERTIFICATE_FILENAME,
                            ADDITIONAL_CERTIFICATE_PASSWORD,
                            HttpStatusCode.Unauthorized,
                            getAccessTokenCertificateFilename: CERTIFICATE_FILENAME,
                            getAccessTokenCertificatePassword: CERTIFICATE_PASSWORD
                            );
        }

        [Theory]
        [InlineData("4")]
        [InlineData("99")]
        public async Task AC10_GetSSA_UnsupportedXV_ShouldRespondWith_406NotAcceptable(string XV)
        {
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.NotAcceptable,
                x_v: XV,
                expectedContent: @"
                    {
                        ""errors"": [
                                {
                                ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                                ""title"": ""Unsupported Version"",
                                ""detail"": ""minimum version: 1, maximum version: 3"",
                                ""meta"": {}
                                }
                            ]
                    }");
        }

        [Theory]
        [InlineData("3", "foo")]
        public async Task AC11_GetSSA_InvalidSoftwareProductId_ShouldRespondWith_404NotFound(string XV, string softwareProductId)
        {
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.NotFound,
                x_v: XV,
                softwareProductId: softwareProductId,
                expectedContent: $@"
                    {{
                        ""errors"": [
                                {{
                                ""code"": ""urn:au-cds:error:cds-register:Field/InvalidSoftwareProduct"",
                                ""title"": ""Invalid Software Product"",
                                ""detail"": ""{softwareProductId}"",
                                ""meta"": {{}}
                                }}
                            ]
                    }}");
        }

        [Theory]
        [InlineData("")]
        public async Task AC12_GetSSA_WithMissingSoftwareProductId_ShouldRespondWith_404NotFound(string softwareProductId)
        {
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.NotFound,
                x_v: "2",
                softwareProductId: softwareProductId);
        }

        [Fact]
        public async Task ACX01_GetSSA_WithRandomBrandId_ShouldRespondWith_404NotFound()
        {
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.NotFound,
                brandId: Guid.NewGuid().ToString()
            );
        }

        private static async Task<JsonWebKeySet> GetSsaJwks()
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var jwksClient = new HttpClient(clientHandler);
            var jwksResponse = await jwksClient.GetAsync($"{TLS_BaseURL}/cdr-register/v1/jwks");
            return new JsonWebKeySet(await jwksResponse.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("99", "3")]
        public async Task ACXX_Get_WithMinXV_ShouldRespondWith_200OK(string xv, string minXv)
        {
            // Act
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.OK,
                x_v: xv,
                x_min_v: minXv,
                expectedXV: "3"
            );
        }

        [Theory]
        [InlineData("2", "2")]
        [InlineData("2", "3")]
        public async Task ACXX_Get_WithMinXVGreaterThanOrEqualToXV_ShouldBeIgnored_200OK(string xv, string minXv)
        {
            // Act
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.OK,
                x_v: xv,
                x_min_v: minXv,
                industry: "banking"
            );
        }

        [Theory]
        [InlineData(null, "2")]
        [InlineData(null, "3")]
        public async Task ACXX_Get_WithMinXVAndNoXV_ShouldBeIgnored_200OK(string xv, string minXv)
        {
            // Act
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.OK,
                x_v: xv,
                x_min_v: minXv,
                expectedXV: "1",
                industry: "banking"
            );
        }

        [Theory]
        [InlineData("99", "foo")]
        [InlineData("99", "0")]
        [InlineData("99", "-1")]
        public async Task ACXX_Get_WithInvalidMinXV_ShouldReturnInvalidVersionError_400BadRequest(string xv, string minXv)
        {
            // Act
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.BadRequest,
                x_v: xv,
                x_min_v: minXv,
                expectedContent: EXPECTEDCONTENT_INVALIDXV
            );
        }

        [Theory]
        [InlineData("99", "10")]
        public async Task ACXX_Get_WithUnsupportedMinXV_ShouldReturnUnsupportedVersionError_406NotAccepted(string xv, string minXv)
        {
            // Arrange
            var expectedContent = EXPECTEDCONTENT_UNSUPPORTEDXV.Replace("#{minVersion}", "1").Replace("#{maxVersion}", "3");

            // Act
            await Test_GetSSA(
                CERTIFICATE_FILENAME,
                CERTIFICATE_PASSWORD,
                HttpStatusCode.NotAcceptable,
                x_v: xv,
                x_min_v: minXv,
                expectedContent: expectedContent
            );
        }
    }
}
