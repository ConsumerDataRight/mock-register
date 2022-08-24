using CDR.Register.IntegrationTests.Infrastructure;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataHolderBrands.
    /// </summary>
    public class US12669_GetDataHolderBrands_Tests : BaseTest
    {
        // Participation/Brand/SoftwareProduct Ids
        private static string PARTICIPATIONID => GetParticipationId(BRANDID); // lookup 
        private const string BRANDID = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SOFTWAREPRODUCTID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        // Get expected data holder brands (for Banking)
        private static string GetExpectedResponse_Banking(string baseUrl, string selfUrl, DateTime? updatedSince, int? requestedPage, int? requestedPageSize)
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

            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var allData = dbContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Where(brand => brand.Participation.ParticipationTypeId == ParticipationTypes.Dh)
                .Where(brand => brand.Participation.IndustryId == Industry.BANKING) // Only want banking brands
                .Where(brand => brand.Participation.LegalEntity.LegalEntityStatusId == LegalEntityStatusType.Active) // DF: only active data holders should be returned.
                .Where(brand => brand.Participation.StatusId == ParticipationStatusType.Active)
                .Where(brand => brand.BrandStatusId == BrandStatusType.Active)
                .Where(brand => updatedSince == null || brand.LastUpdated > updatedSince);

            var data = allData
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(brand => new
                {
                    dataHolderBrandId = brand.BrandId,
                    brandName = brand.BrandName,
                    industry = brand.Participation.Industry.IndustryTypeCode.ToLower(),
                    logoUri = brand.LogoUri,
                    legalEntity = new
                    {
                        legalEntityId = brand.Participation.LegalEntity.LegalEntityId,
                        legalEntityName = brand.Participation.LegalEntity.LegalEntityName,
                        logoUri = brand.Participation.LegalEntity.LogoUri,
                        registrationNumber = brand.Participation.LegalEntity.RegistrationNumber,
                        registrationDate = brand.Participation.LegalEntity.RegistrationDate == null ? null : brand.Participation.LegalEntity.RegistrationDate.Value.ToString("yyyy-MM-dd"),
                        registeredCountry = brand.Participation.LegalEntity.RegisteredCountry,
                        abn = brand.Participation.LegalEntity.Abn,
                        acn = brand.Participation.LegalEntity.Acn,
                        arbn = brand.Participation.LegalEntity.Arbn,
                        industryCode = brand.Participation.LegalEntity.AnzsicDivision,
                        organisationType = brand.Participation.LegalEntity.OrganisationType.OrganisationTypeCode
                    },
                    status = brand.BrandStatus.BrandStatusCode,
                    endpointDetail = new
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

        private static async Task Test_AC01_AC02_AC03_AC04_AC05_AC06(DateTime? updatedSince, int? queryPage, int? queryPageSize)
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

            var expectedResponse = GetExpectedResponse_Banking(baseUrl, url, updatedSince, queryPage, queryPageSize);

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
        public async Task AC01_Get_WithAccessToken_AndNoQueryString_ShouldRespondWith_200OK_First25RecordsAsync()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, null, null);
        }

        [Fact]
        public async Task AC02_Get_WithAccessToken_AndPageSize5_ShouldRespondWith_200OK_Page1Of5Records()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, null, 5);
        }

        [Fact]
        public async Task AC03_Get_WithAccessToken_AndPageSize5_AndPage3_ShouldRespondWith_200OK_Page3Of5Records()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, 3, 5);
        }

        [Fact]
        public async Task AC04_Get_WithAccessToken_AndPageSize5_AndPage6_ShouldRespondWith_200OK_Page6Of5Records()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, 6, 5);
        }

        [Fact]
        public async Task AC05_Get_WithAccessToken_AndUpdatedSince01042021_ShouldRespondWith_200OK_2Records()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(new DateTime(2021, 04, 01, 0, 0, 0, DateTimeKind.Utc), 6, 5);
        }

        [Fact]
        public async Task AC06_Get_WithAccessToken_AndUpdatedSince30042021_ShouldRespondWith_200OK_0Records()
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(new DateTime(2021, 04, 30, 0, 0, 0, DateTimeKind.Utc), null, null);
        }

        [Theory]
        [InlineData("", HttpStatusCode.OK)] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest)]
        [InlineData("32/32/2021", HttpStatusCode.BadRequest)]
        public async Task AC07_Get_WithAccessToken_AndUpdatedSinceInvalidDate_ShouldRespondWith_400BadRequest_InvalidDateTimeString(string updatedSince, HttpStatusCode expectedStatusCode)
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
                            ""code"": ""urn:au-cds:error:cds-all:Field/InvalidDateTime"",
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
        public async Task AC08_Get_WithAccessToken_AndInvalidIndustry_ShouldRespondWith_400BadRequest(string industry, HttpStatusCode expectedStatusCode)
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
                                ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                                ""title"": ""Invalid Field"",
                                ""detail"": ""industry"",
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
        [InlineData("3", HttpStatusCode.NotAcceptable)]
        public async Task AC09_Get_WithAccessToken_AndInvalidXV_ShouldRespondWith_406NotAcceptable(string xv, HttpStatusCode expectedStatusCode)
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
                            ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                            ""title"": ""Unsupported Version"",
                            ""detail"": ""minimum version: 1, maximum version: 2"",
                            ""meta"": {}
                            }
                        ]
                    }";
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        private static async Task Test_AC10_AC11(string? accessToken)
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
        public async Task AC10_Get_WithNoAccessToken_ShouldRespondWith_401Unauthorized()
        {
            await Test_AC10_AC11(null);
        }

        [Fact]
        public async Task AC11_Get_WithInvalidAccessToken_ShouldRespondWith_401Unauthorized()
        {
            await Test_AC10_AC11("foo");
        }

        [Fact]
        public async Task AC13_Get_WithDifferentHolderOfKey_ShouldRespondWith_401Unauthorized()
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

        private static async Task Test_AC14_AC15_AC16_AC17(string queryString, HttpStatusCode expectedStatusCode, string expectedContent)
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
        public async Task AC14_Get_WithAccessToken_AndInvalidPageSize_ShouldRespondWith_400BadRequest_PageSizeMustBePositiveInteger(string pageSize, HttpStatusCode expectedStatusCode)
        {
            await Test_AC14_AC15_AC16_AC17($"page-size={pageSize}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/InvalidPageSize"",
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
        public async Task AC15_Get_WithAccessToken_AndInvalidPage_ShouldRespondWith_400BadRequest_PageMustBePositiveInteger(string page, HttpStatusCode expectedStatusCode)
        {
            await Test_AC14_AC15_AC16_AC17($"page={page}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page not a positive integer"",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        [Theory]
        [InlineData("3")]
        public async Task AC16_Get_WithAccessToken_AndPageOutOfRange_ShouldRespondWith_400BadRequest_PageExceedsMaxNumberOfPages(string page)
        {
            await Test_AC14_AC15_AC16_AC17($"page={page}", HttpStatusCode.BadRequest, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page is out of range"",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        [Theory]
        [InlineData("1001")]
        public async Task AC17_Get_WithAccessToken_AndPageSizeTooLarge_ShouldRespondWith_400BadRequest_PageSizeTooLarge(string pageSize)
        {
            await Test_AC14_AC15_AC16_AC17($"page-size={pageSize}", HttpStatusCode.BadRequest, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page size too large"",
                        ""meta"": {}
                        }
                    ]
                }");
        }

        delegate void BeforeTestAC181920();
        delegate void AfterTestAC181920Request();
        private static async Task Test_AC18_AC19_AC20(HttpStatusCode expectedStatusCode,
           BeforeTestAC181920? beforeRequest,
           AfterTestAC181920Request? afterRequest)
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
                                    ""code"": ""urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive"",
                                    ""title"": ""ADR Status Is Not Active"",
                                    ""detail"": """",
                                    ""meta"": {}
                                    }
                                ]
                            }";
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
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden)] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden)] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden)] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden)] // Inactive         
        public async Task AC18_Get_WithAccessToken_AndDataRecipientNotActive_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode)
        {
            var saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Test_AC18_AC19_AC20(
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId));
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task AC19_Get_WithAccessToken_AndDataRecipientBrandNotActive_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Test_AC18_AC19_AC20(
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId)
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.OK)]        // Active
        [InlineData(2, HttpStatusCode.Forbidden)] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden)] // Removed 
        public async Task AC20_Get_WithAccessToken_AndDataRecipientSoftwareProductNotActive_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Test_AC18_AC19_AC20(
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId)
            );
        }

        [Fact]
        public async Task AC21_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD
            }.GetAsync();

            // Arrange - Get brands and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act - Use Etag
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = "1",
                IfNoneMatch = expectedETag,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotModified);

                // Assert - No content
                (await response.Content.ReadAsStringAsync()).Should().BeNullOrEmpty();
            }
        }
    }
}
