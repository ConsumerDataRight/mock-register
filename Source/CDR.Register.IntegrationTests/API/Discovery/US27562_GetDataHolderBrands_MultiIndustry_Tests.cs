﻿using CDR.Register.IntegrationTests.Infrastructure;
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
    public class US27562_GetDataHolderBrands_MultiIndustry_Tests : BaseTest
    {
        // Participation/Brand/SoftwareProduct Ids
        private static string PARTICIPATIONID => GetParticipationId(BRANDID); // lookup 
        private const string BRANDID = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SOFTWAREPRODUCTID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        private static string GetExpectedResponse(string baseUrl, string selfUrl, DateTime? updatedSince, int? requestedPage, int? requestedPageSize, string? industry = null)
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

            if (industry == "all")
            {
                industry = null; // treat "all" same as no industry
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
                .Where(brand =>
                    brand.Participation.ParticipationTypeId == ParticipationTypes.Dh &&
                    (industry == null || (industry != null && brand.Participation.Industry.IndustryTypeCode == industry))
                )
                .Where(brand => brand.Participation.StatusId == ParticipationStatusType.Active)
                .Where(brand => brand.BrandStatusId == BrandStatusType.Active)
                .Where(brand => updatedSince == null || brand.LastUpdated > updatedSince);

            var totalRecords = allData.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            const int MINPAGE = 1;
            if (page < MINPAGE)
            {
                throw new Exception($"Page {page} out of range. Min Page is {MINPAGE}");
            }
            var maxPage = ((totalRecords - 1) / pageSize) + 1;
            if (page > maxPage)
            {
                throw new Exception($"Page {page} out of range. Max Page is {maxPage} (Records={totalRecords}, PageSize={pageSize})");
            }

            var data = allData
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(brand => new
                {
                    dataHolderBrandId = brand.BrandId,
                    brandName = brand.BrandName,
                    industries = new string[] { brand.Participation.Industry.IndustryTypeCode.ToLower() }, //DF: industry should be lowercase.
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
                        anzsicDivision = brand.Participation.LegalEntity.AnzsicDivision,
                        organisationType = brand.Participation.LegalEntity.OrganisationType.OrganisationTypeCode,
                        status = brand.Participation.Status.ParticipationStatusCode.ToUpper() //DF: status should be uppercase. //TODO:C could fail tests
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

            return JsonConvert.SerializeObject(expectedResponse);
        }

        private static async Task Test_AC01_AC02_AC03_AC04_AC05_AC06(
            DateTime? updatedSince,
            int? queryPage,
            int? queryPageSize,
            string? industry = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
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
            var baseUrl = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands";
            var url = GetUrl(baseUrl, updatedSince, queryPage, queryPageSize);

            var expectedResponse = GetExpectedResponse(baseUrl, url, updatedSince, queryPage, queryPageSize, industry);

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
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check XV
                    Assert_HasHeader(api.XV, response.Headers, "x-v");

                    // Assert - Check json
                    await Assert_HasContent_Json(expectedResponse, response.Content);
                }
            }
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC01_Get_WithNoQueryString_ShouldRespondWith_200OK_First25RecordsAsync(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, null, null, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        public async Task AC02_Get_WithPageSize5_ShouldRespondWith_200OK_Page1Of5Records(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, null, 5, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        public async Task AC03_Get_WithPageSize5_AndPage3_ShouldRespondWith_200OK_Page3Of5Records(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, 3, 5, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        public async Task AC04_Get_WithPageSize5_AndPage6_ShouldRespondWith_200OK_Page6Of5Records(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(null, 6, 5, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        public async Task AC05_Get_WithUpdatedSince01042021_ShouldRespondWith_200OK_2Records(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(new DateTime(2021, 04, 01, 0, 0, 0, DateTimeKind.Utc), 6, 5, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC06_Get_WithUpdatedSince30042021_ShouldRespondWith_200OK_0Records(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_AC01_AC02_AC03_AC04_AC05_AC06(new DateTime(2021, 04, 30, 0, 0, 0, DateTimeKind.Utc), null, null, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.NotFound, null)] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "telco")]
        public async Task AC07_Get_WithUpdatedSinceInvalidDate_ShouldRespondWith_400BadRequest_InvalidDateTimeString(string updatedSince, HttpStatusCode expectedStatusCode, string? industry)
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
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands?updated-since={updatedSince}",
                AccessToken = accessToken,
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
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

        private static async Task Test_AC08_AC20_AC21(string xv, HttpStatusCode expectedStatusCode, string expectedContent, string? industry)
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
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response                
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData("2", HttpStatusCode.NotFound, null)] // DF: will return a 404.
        [InlineData("3", HttpStatusCode.NotFound, null)] // DF: will return a 404.
        [InlineData("2", HttpStatusCode.OK, "banking")]
        [InlineData("3", HttpStatusCode.NotAcceptable, "banking")]
        [InlineData("2", HttpStatusCode.OK, "energy")]
        [InlineData("3", HttpStatusCode.NotAcceptable, "energy")]
        [InlineData("2", HttpStatusCode.OK, "telco")]
        [InlineData("3", HttpStatusCode.NotAcceptable, "telco")]
        public async Task AC08_Get_WithUnsupportedXV_ShouldRespondWith_406NotAcceptable(string xv, HttpStatusCode expectedStatusCode, string? industry)
        {
            await Test_AC08_AC20_AC21(xv, expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                        ""title"": ""Unsupported Version"",
                        ""detail"": ""minimum version: 1, maximum version: 2"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        private static async Task Test_AC09_AC10(string? accessToken, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC09_Get_WithNoAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            await Test_AC09_AC10(null, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC10_Get_WithInvalidAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            await Test_AC09_AC10("foo", industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC11_Get_WithExpiredAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            // Arrange
            // Expired at "Tuesday, May 18, 2021 11:33:45 PM GMT+10:00"
            var accessToken = "eyJhbGciOiJQUzI1NiIsImtpZCI6IkFBMjRGMTg1RUUzRjY3NTA0ODA4RkM0RTI2QjEzNUI5OUU2M0JEQTkiLCJ0eXAiOiJhdCtqd3QiLCJ4NXQiOiJxaVR4aGU0X1oxQklDUHhPSnJFMXVaNWp2YWsifQ.eyJuYmYiOjE2MjEzNDQ1MjUsImV4cCI6MTYyMTM0NDgyNSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzAwMC9pZHAiLCJhdWQiOiJjZHItcmVnaXN0ZXIiLCJjbGllbnRfaWQiOiI2ZjdhMWI4ZS04Nzk5LTQ4YTgtOTAxMS1lMzkyMDM5MWY3MTMiLCJqdGkiOiJDODRBNTM5MTA2QjI4NUJBODI2RjZGMDQ3MjU4RjBBNCIsImlhdCI6MTYyMTM0NDUyNSwic2NvcGUiOlsiY2RyLXJlZ2lzdGVyOmJhbms6cmVhZCJdLCJjbmYiOnsieDV0I1MyNTYiOiI1OEQ3NkY3QTYxQ0Q3MjZEQTFDNTRGNjg5OEU4RTY5RUE0Qzg4MDYwIn19.RTU-zrqkb-WXcJzCz62SJ4h19lj8MDyGcvLOmg0qx05WFbAsY4mEP3gsoqM1LJfq4ncw7RqSvbkCNQQ-NOnyoBHF8MGe7mzdUh3YrD0_lTg20Dkx1-l044svtP_CKTI3rXT3bZaYWce0Tb1s3mrJzfN3ja23o93FGR-wbIwHp2347b0DxjznpKBw5meLhAjS7OCx6_uMm1la6IziSQgqMd2WaA-od7w8J5br-Nn-QZZi7X1KGiPEKFDFNk8KrUdPc4NCH6t7f-Sbc34KNNEWfAOJkWdDrmsBaifSlWvSlS4nUnurGHYkmimA2JUuv3ZTqzCcLRamEER1ZoTcIs_PDw";

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("telco")]
        public async Task AC12_Get_WithDifferentHolderOfKey_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
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
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        private static async Task Test_AC13_AC14_AC15_AC16(string queryString, HttpStatusCode expectedStatusCode, string expectedContent, string? industry)
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
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands?{queryString}",
                AccessToken = accessToken,
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                {
                    // Assert - Check content type 
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response                
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.NotFound, null)]
        [InlineData("-1", HttpStatusCode.NotFound, null)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.NotFound, null)]
        [InlineData("foo", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "banking")]
        [InlineData("-1", HttpStatusCode.BadRequest, "banking")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "banking")]
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "energy")]
        [InlineData("-1", HttpStatusCode.BadRequest, "energy")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "energy")]
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "telco")]
        [InlineData("-1", HttpStatusCode.BadRequest, "telco")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "telco")]
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")]
        public async Task AC13_Get_WithInvalidPageSize_ShouldRespondWith_400BadRequest_PageSizeMustBePositiveInteger(string pageSize, HttpStatusCode expectedStatusCode, string? industry)
        {
            await Test_AC13_AC14_AC15_AC16($"page-size={pageSize}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/InvalidPageSize"",
                        ""title"": ""Invalid Page Size"",
                        ""detail"": ""Page size not a positive Integer"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.NotFound, null)]
        [InlineData("-1", HttpStatusCode.NotFound, null)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.NotFound, null)]
        [InlineData("foo", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "banking")]
        [InlineData("-1", HttpStatusCode.BadRequest, "banking")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "banking")]
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "energy")]
        [InlineData("-1", HttpStatusCode.BadRequest, "energy")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "energy")]
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "telco")]
        [InlineData("-1", HttpStatusCode.BadRequest, "telco")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "telco")]
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")]
        public async Task AC14_Get_WithInvalidPage_ShouldRespondWith_400BadRequest_PageMustBePositiveInteger(string page, HttpStatusCode expectedStatusCode, string? industry)
        {
            await Test_AC13_AC14_AC15_AC16($"page={page}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page not a positive integer"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        [Theory]
        [InlineData("3", null, HttpStatusCode.NotFound)]
        [InlineData("3", "banking")]
        [InlineData("3", "energy")]
        [InlineData("3", "telco")]
        public async Task AC15_Get_WithPageOutOfRange_ShouldRespondWith_400BadRequest_PageExceedsMaxNumberOfPages(string page, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            await Test_AC13_AC14_AC15_AC16($"page={page}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page is out of range"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        [Theory]
        [InlineData("1001", null, HttpStatusCode.NotFound)]
        [InlineData("1001", "banking")]
        [InlineData("1001", "energy")]
        [InlineData("1001", "telco")]
        public async Task AC16_Get_WithPageSizeTooLarge_ShouldRespondWith_400BadRequest_PageSizeTooLarge(string pageSize, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            await Test_AC13_AC14_AC15_AC16($"page-size={pageSize}", expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""Page size too large"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        delegate void BeforeTestAC181920();
        delegate void AfterTestAC181920Request();
        private static async Task Test_AC17_AC18_AC19(HttpStatusCode expectedStatusCode,
           BeforeTestAC181920? beforeRequest,
           AfterTestAC181920Request? afterRequest,
           string? industry)
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
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2"
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
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
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
        [InlineData(1, HttpStatusCode.NotFound, null)]        // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Removed 
        [InlineData(3, HttpStatusCode.NotFound, null)] // Suspended 
        [InlineData(4, HttpStatusCode.NotFound, null)] // Revoked 
        [InlineData(5, HttpStatusCode.NotFound, null)] // Surrendered
        [InlineData(6, HttpStatusCode.NotFound, null)] // Inactive         
        [InlineData(1, HttpStatusCode.OK, "banking")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden, "banking")] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden, "banking")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "banking")] // Inactive         
        [InlineData(1, HttpStatusCode.OK, "energy")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden, "energy")] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden, "energy")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "energy")] // Inactive         
        [InlineData(1, HttpStatusCode.OK, "telco")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Removed 
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Suspended 
        [InlineData(4, HttpStatusCode.Forbidden, "telco")] // Revoked 
        [InlineData(5, HttpStatusCode.Forbidden, "telco")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "telco")] // Inactive         
        public async Task ACX17_Get_WithDataRecipientNotActive_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            var saveParticipationStatusId = GetParticipationStatusId(PARTICIPATIONID);

            await Test_AC17_AC18_AC19(
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(PARTICIPATIONID, participationStatusId),
                afterRequest: () => SetParticipationStatusId(PARTICIPATIONID, saveParticipationStatusId),
                industry: industry);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NotFound, null)]        // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Inactive 
        [InlineData(3, HttpStatusCode.NotFound, null)] // Removed 
        [InlineData(1, HttpStatusCode.OK, "banking")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Removed 
        [InlineData(1, HttpStatusCode.OK, "energy")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Removed 
        [InlineData(1, HttpStatusCode.OK, "telco")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Removed 
        public async Task ACX18_Get_WithDataRecipientBrandNotActive_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            int saveBrandStatusId = GetBrandStatusId(BRANDID);

            await Test_AC17_AC18_AC19(
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BRANDID, brandStatusId),
                afterRequest: () => SetBrandStatusId(BRANDID, saveBrandStatusId),
                industry: industry
            );
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NotFound, null)]        // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Inactive 
        [InlineData(3, HttpStatusCode.NotFound, null)] // Removed 
        [InlineData(1, HttpStatusCode.OK, "banking")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Removed 
        [InlineData(1, HttpStatusCode.OK, "energy")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Removed 
        [InlineData(1, HttpStatusCode.OK, "telco")]        // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Inactive 
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Removed 
        public async Task ACX19_Get_WithDataRecipientSoftwareProductNotActive_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SOFTWAREPRODUCTID);

            await Test_AC17_AC18_AC19(
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SOFTWAREPRODUCTID, saveSoftwareProductStatusId),
                industry: industry
            );
        }

        [Theory]
        [InlineData("all")]
        public async Task ACX20_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag(string? industry)
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
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act - Use Etag
            var response = await new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2",
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

        [Theory]
        [InlineData("foo", null, HttpStatusCode.NotFound)]
        [InlineData("foo", "banking")]
        [InlineData("foo", "energy")]
        [InlineData("foo", "telco")]
        public async Task AC20_Get_WithInvalidXV_ShouldRespondWith_400BadRequest(string xv, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            await Test_AC08_AC20_AC21(xv, expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Header/InvalidVersion"",
                        ""title"": ""Invalid Version"",
                        ""detail"": """",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        [Theory]
        [InlineData(null, null, HttpStatusCode.NotFound)]
        [InlineData(null, "banking", HttpStatusCode.OK)] // DF: this will call the x-v: 1 version and will be successful.
        [InlineData(null, "energy")]
        [InlineData(null, "telco")]
        public async Task AC21_Get_WithMissingXV_ShouldRespondWith_400BadRequest(string xv, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            await Test_AC08_AC20_AC21(xv, expectedStatusCode, @"
                {
                    ""errors"": [
                        {
                        ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                        ""title"": ""Invalid Field"",
                        ""detail"": ""industry"",
                        ""meta"": {}
                        }
                    ]
                }",
                industry);
        }

        [Theory]
        [InlineData("foo")]
        public async Task ACX01_Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest(string industry)
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
                XV = "2"
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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
        [InlineData(null, "cdr-register:bank:read", HttpStatusCode.NotFound)] // No industry
        [InlineData(null, "cdr-register:read", HttpStatusCode.NotFound)] // No industry
        [InlineData("banking", "cdr-register:bank:read", HttpStatusCode.Forbidden)]
        [InlineData("banking", "cdr-register:read", HttpStatusCode.OK)]
        [InlineData("energy", "cdr-register:bank:read", HttpStatusCode.Forbidden)]  // ???
        [InlineData("energy", "cdr-register:read", HttpStatusCode.OK)]
        [InlineData("telco", "cdr-register:bank:read", HttpStatusCode.Forbidden)]  // ???
        [InlineData("telco", "cdr-register:read", HttpStatusCode.OK)]
        public async Task ACX02_Get_WithScope_ShouldRespondWith_200OK(string industry, string scope, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = scope
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = "2",
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Theory]
        [InlineData("99", "2")]
        public async Task ACXX_Get_WithMinXV_ShouldRespondWith_200OK(string xv, string minXv)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/all/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv
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

                // Assert - Check error response
                Assert_HasHeader("2", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("2", "2")]
        [InlineData("2", "3")]
        public async Task ACXX_Get_WithMinXVGreaterThanOrEqualToXV_ShouldBeIgnored_200OK(string xv, string minXv)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/all/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv
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

                // Assert - Check error response
                Assert_HasHeader("2", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData(null, "2")]
        [InlineData(null, "3")]
        public async Task ACXX_Get_WithMinXVAndNoXV_ShouldBeIgnored_200OK(string xv, string minXv)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/banking/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv,
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

                // Assert - Check error response
                Assert_HasHeader("1", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("99", "foo")]
        [InlineData("99", "0")]
        [InlineData("99", "-1")]
        public async Task ACXX_Get_WithInvalidMinXV_ShouldReturnInvalidVersionError_400BadRequest(string xv, string minXv)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/all/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                await Assert_HasContent_Json(EXPECTEDCONTENT_INVALIDXV, response.Content);
            }
        }

        [Theory]
        [InlineData("99", "10")]
        public async Task ACXX_Get_WithUnsupportedMinXV_ShouldReturnUnsupportedVersionError_406NotAccepted(string xv, string minXv)
        {
            // Arrange
            var expectedContent = EXPECTEDCONTENT_UNSUPPORTEDXV.Replace("#{minVersion}", "1").Replace("#{maxVersion}", "2");

            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.API
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/all/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
    }
}
