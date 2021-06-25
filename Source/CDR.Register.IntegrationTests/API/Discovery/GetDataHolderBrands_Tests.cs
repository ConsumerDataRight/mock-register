using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Infrastructure;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataHolderBrands.
    /// </summary>
    public class GetDataHolderBrands_Tests : BaseTest
    {
        // Participation/Brand/SoftwareProduct Ids
        static private string PARTICIPATIONID => GetParticipationId(BRANDID); // lookup 
        private const string BRANDID = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SOFTWAREPRODUCTID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        private static string GetExpectedResponse(string baseUrl, string selfUrl, DateTime? updatedSince, int? requestedPage, int? requestedPageSize)
        {
            static string Link(string baseUrl, DateTime? updatedSince, int? page = null, int? pageSize = null)
            {
                var query = new KeyValuePairBuilder();

                if (updatedSince != null)
                {
                    query.Add("updated-since", ((DateTime)updatedSince).ToString("yyyy-MM-ddTHH\\%3Amm\\%3Ass.fffffffZ"));
                }

                if (page != null)
                {
                    query.Add("page", page.Value);
                }

                if (pageSize != null)
                {
                    query.Add("page-size", pageSize.Value);
                }

                return query.Count == 0 ?
                    baseUrl :
                    $"{baseUrl}?{query.Value}";
            }

            var page = requestedPage ?? 1;
            var pageSize = requestedPageSize ?? 25;

            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlite(SQLITECONNECTIONSTRING).Options);

            var allData = dbContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Where(brand => brand.Participation.ParticipationTypeId == ParticipationTypeEnum.Dh)
                .Where(brand => updatedSince == null || brand.LastUpdated > updatedSince);

            var data = allData
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(brand => new
                {
                    dataHolderBrandId = brand.BrandId,
                    brandName = brand.BrandName,
                    logoUri = brand.LogoUri,
                    legalEntity = new
                    {
                        legalEntityId = brand.Participation.LegalEntity.LegalEntityId,
                        legalEntityName = brand.Participation.LegalEntity.LegalEntityName,
                        industry = brand.DataHolder.Industry,
                        logoUri = brand.Participation.LegalEntity.LogoUri,
                        registrationNumber = brand.Participation.LegalEntity.RegistrationNumber,
                        registrationDate = brand.Participation.LegalEntity.RegistrationDate,
                        registeredCountry = brand.Participation.LegalEntity.RegisteredCountry,
                        abn = brand.Participation.LegalEntity.Abn,
                        acn = brand.Participation.LegalEntity.Acn,
                        arbn = brand.Participation.LegalEntity.Arbn,
                        industryCode = brand.Participation.LegalEntity.IndustryCode,
                        organisationType = brand.Participation.LegalEntity.OrganisationType.OrganisationTypeCode
                    },
                    status = brand.BrandStatus.BrandStatusCode,
                    endPointDetail = new
                    {
                        version = brand.Endpoint.Version,
                        publicBaseUri = brand.Endpoint.PublicBaseUri,
                        resourceBaseUri = brand.Endpoint.ResourceBaseUri,
                        infosecBaseUri = brand.Endpoint.InfosecBaseUri,
                        extensionBaseUri = brand.Endpoint.ExtensionBaseUri,
                        websiteUri = brand.Endpoint.WebsiteUri
                    },
                    authDetails = brand.AuthDetails.Select(authDetails => new
                    {
                        registerUType = authDetails.RegisterUType.RegisterUTypeCode,
                        jwksEndpoint = authDetails.JwksEndpoint
                    }),
                    lastUpdated = brand.LastUpdated.ToUniversalTime()
                })
                .ToList();

            var totalRecords = allData.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var expectedResponse = new
            {
                data,
                links = new
                {
                    first = totalPages == 0 ?
                        null :
                        Link(baseUrl, updatedSince, 1, pageSize),
                    last = totalPages == 0 ?
                        null :
                        Link(baseUrl, updatedSince, totalPages, pageSize),
                    next = totalPages == 0 || page == totalPages ?
                        null :
                        Link(baseUrl, updatedSince, page + 1, pageSize),
                    prev = totalPages == 0 || page == 1 ?
                        null :
                        Link(baseUrl, updatedSince, page - 1, pageSize),
                    self = selfUrl,
                },
                meta = new
                {
                    totalRecords,
                    totalPages
                }
            };

            return JsonConvert.SerializeObject(expectedResponse,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });
        }

        private static async Task Get_WithAccessToken_Tests(DateTime? updatedSince, int? queryPage, int? queryPageSize)
        {
            static string GetUrl(string baseUrl, DateTime? updatedSince, int? queryPage, int? queryPageSize)
            {
                // Build query
                var query = new KeyValuePairBuilder();

                if (updatedSince != null)
                {
                    query.Add("updated-since", ((DateTime)updatedSince).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                }

                if (queryPage != null)
                {
                    query.Add("page", queryPage.Value);
                }
                if (queryPageSize != null)
                {
                    query.Add("page-size", queryPageSize.Value);
                }

                return query.Count > 0 ?
                    $"{baseUrl}?{query.Value}" :
                    baseUrl;
            }

            // Arrange
            var baseUrl = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands";
            var url = GetUrl(baseUrl, updatedSince, queryPage, queryPageSize);

            var expectedResponse = GetExpectedResponse(baseUrl, url, updatedSince, queryPage, queryPageSize);

            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = url,
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader(api.XV, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedResponse, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithAccessToken_AndNoQueryString_ShouldRespondWith_200OK_First25RecordsAsync()
        {
            await Get_WithAccessToken_Tests(null, null, null);
        }

        [Fact]
        public async Task Get_WithAccessToken_AndPageSize5_ShouldRespondWith_200OK_Page1Of5Records()
        {
            await Get_WithAccessToken_Tests(null, null, 5);
        }

        [Fact]
        public async Task Get_WithAccessToken_AndPageSize5_AndPage3_ShouldRespondWith_200OK_Page3Of5Records()
        {
            await Get_WithAccessToken_Tests(null, 3, 5);
        }

        [Fact]
        public async Task Get_WithAccessToken_AndPageSize5_AndPage6_ShouldRespondWith_200OK_Page6Of5Records()
        {
            await Get_WithAccessToken_Tests(null, 6, 5);
        }

        [Fact]
        public async Task Get_WithAccessToken_AndUpdatedSince01042021_ShouldRespondWith_200OK_2Records()
        {
            await Get_WithAccessToken_Tests(new DateTime(2021, 04, 01, 0, 0, 0, DateTimeKind.Utc), 6, 5);
        }

        [Fact]
        public async Task Get_WithAccessToken_AndUpdatedSince30042021_ShouldRespondWith_200OK_0Records()
        {
            await Get_WithAccessToken_Tests(new DateTime(2021, 04, 30, 0, 0, 0, DateTimeKind.Utc), null, null);
        }

        [Theory]
        [InlineData("", HttpStatusCode.OK)] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest)]
        [InlineData("32/32/2021", HttpStatusCode.BadRequest)]
        public async Task Get_WithAccessToken_AndUpdatedSinceInvalidDate_ShouldRespondWith_400BadRequest_InvalidDateTimeString(string updatedSince, HttpStatusCode expectedStatusCode)
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands?updated-since={updatedSince}",
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    var expectedContent = @"
                    {
                        ""errors"": [
                            {
                            ""code"": ""Field/InvalidDateTime"",
                            ""title"": ""Invalid DateTime"",
                            ""detail"": ""updated-since should be valid DateTimeString"",
                            ""meta"": {}
                            }
                        ]
                    }";
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound)]
        [InlineData("foo", HttpStatusCode.BadRequest)]
        public async Task Get_WithAccessToken_AndInvalidIndustry_ShouldRespondWith_400BadRequest(string industry, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    var expectedContent = @"
                    {
                        ""errors"": [
                                {
                                ""code"": ""Field/InvalidIndustry"",
                                ""title"": ""Invalid Industry"",
                                ""detail"": """",
                                ""meta"": {}
                                }
                            ]
                    }";
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData("", HttpStatusCode.OK)] // "" will effectively be no version specified, so it will default to 1
        [InlineData("2", HttpStatusCode.NotAcceptable)]
        public async Task Get_WithAccessToken_AndInvalidXV_ShouldRespondWith_406NotAcceptable(string xv, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = xv
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    var expectedContent = @"
                    {
                        ""errors"": [
                            {
                            ""code"": ""Header/UnsupportedVersion"",
                            ""title"": ""Unsupported Version"",
                            ""detail"": """",
                            ""meta"": {}
                            }
                        ]
                    }";
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        private static async Task Get_UsingAccessToken_Tests(string? accessToken)
        {
            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task Get_WithNoAccessToken_ShouldRespondWith_401Unauthorized()
        {
            await Get_UsingAccessToken_Tests(null);
        }

        [Fact]
        public async Task Get_WithInvalidAccessToken_ShouldRespondWith_401Unauthorized()
        {
            await Get_UsingAccessToken_Tests("foo");
        }

        [Fact]
        public async Task Get_WithDifferentHolderOfKey_ShouldRespondWith_401Unauthorized()
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = ADDITIONAL_CERTIFICATE_FILENAME,  // ie different holder of key
                CertificatePassword = ADDITIONAL_CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        private static async Task Get_WithQueryString_Tests(string queryString, HttpStatusCode expectedStatusCode, string expectedContent)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands?{queryString}",
                AccessToken = accessToken,
                XV = "1"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response                
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData("", HttpStatusCode.OK)] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest)]
        [InlineData("-1", HttpStatusCode.BadRequest)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest)]
        [InlineData("foo", HttpStatusCode.BadRequest)]
        public async Task Get_WithAccessToken_AndInvalidPageSize_ShouldRespondWith_400BadRequest_PageSizeMustBePositiveInteger(string pageSize, HttpStatusCode expectedStatusCode)
        {
            await Get_WithQueryString_Tests($"page-size={pageSize}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""Field/InvalidPageSize"",
                        ""title"": ""Invalid Page Size"",
                        ""detail"": ""Page size not a positive Integer"",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        [Theory]
        [InlineData("", HttpStatusCode.OK)] // "" is effectively not providing a page, so will default to 1.
        [InlineData("0", HttpStatusCode.BadRequest)]
        [InlineData("-1", HttpStatusCode.BadRequest)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest)]
        [InlineData("foo", HttpStatusCode.BadRequest)]
        public async Task Get_WithAccessToken_AndInvalidPage_ShouldRespondWith_400BadRequest_PageMustBePositiveInteger(string page, HttpStatusCode expectedStatusCode)
        {
            await Get_WithQueryString_Tests($"page={page}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""Field/InvalidPage"",
                        ""title"": ""Invalid Page"",
                        ""detail"": ""Page not a positive Integer"",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        [Theory]
        [InlineData("3")]
        public async Task Get_WithAccessToken_AndPageOutOfRange_ShouldRespondWith_400BadRequest_PageExceedsMaxNumberOfPages(string page)
        {
            await Get_WithQueryString_Tests($"page={page}", HttpStatusCode.BadRequest, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""Field/InvalidPageOutOfRange"",
                        ""title"": ""Page Is Out Of Range"",
                        ""detail"": """",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        [Theory]
        [InlineData("1001")]
        public async Task Get_WithAccessToken_AndPageSizeTooLarge_ShouldRespondWith_400BadRequest_PageSizeTooLarge(string pageSize)
        {
            await Get_WithQueryString_Tests($"page-size={pageSize}", HttpStatusCode.BadRequest, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""Field/InvalidPageSizeTooLarge"",
                        ""title"": ""Page Size Too Large"",
                        ""detail"": """",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        delegate void BeforeCheckDataRecipientStatus();
        delegate void AfterCheckDataRecipientStatusRequest();
        private static async Task Get_CheckDataRecipientStatus_Tests(HttpStatusCode expectedStatusCode,
           BeforeCheckDataRecipientStatus? beforeRequest,
           AfterCheckDataRecipientStatusRequest? afterRequest)
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = "1"
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
                }
            }
            finally
            {
                afterRequest?.Invoke();
            }
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden)] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden)] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden)] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden)] // Inactive         
        public async Task Get_WithAccessToken_AndDataRecipientNotActive_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode)
        {
            var saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Get_CheckDataRecipientStatus_Tests(expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId));
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task Get_WithAccessToken_AndDataRecipientBrandNotActive_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Get_CheckDataRecipientStatus_Tests(
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId)
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task Get_WithAccessToken_AndDataRecipientSoftwareProductNotActive_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Get_CheckDataRecipientStatus_Tests(expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId)
            );
        }
    }
}
