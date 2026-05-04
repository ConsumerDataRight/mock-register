using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.API.Update;
using CDR.Register.Repository.Enums;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Status
{
    /// <summary>
    /// Integration tests for Get Data Holder Status V2 endpoints.
    /// </summary>
    public class GetDataHolderStatusXV2_MultiIndustry_Tests : BaseTest
    {
        /// <summary>
        /// The supported industry in V3.
        /// </summary>
        private const string SupportedIndustry = "all";

        /// <summary>
        /// The supported version - 3.
        /// </summary>
        private const string SupportedVersion = "2";

        /// <summary>
        /// The previous endpoint version which is still supported.
        /// </summary>
        private const string LegacyEndpointVersion = "1";

        /// <summary>
        /// The future (not implemented) endpoint version, which isn't supported.
        /// </summary>
        private const string UnsupportedEndpointVersion = "5";

        public GetDataHolderStatusXV2_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture)
            : base(outputHelper, testFixture)
        {
        }

        [Theory]
        [InlineData(SupportedVersion, SupportedVersion)]
        public async Task Get_ShouldRespondWith_200OK_DataHolderStatuses(string? xv, string expectedXV)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-holders/status";
            var expected = GetExpectedDataHolderStatuses(url);

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = xv,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader(expectedXV, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expected, response.Content);
            }
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(SupportedVersion, SupportedVersion)]
        public async Task Get_WithDynamicPaths_ShouldRespondWith_200OK_DataHolderStatuses(string? xv, string expectedXV)
        {
            // Arrange
            var url = $"{GenerateDynamicCtsUrl(STATUS_DOWNSTREAM_BASE_URL)}/cdr-register/v1/{SupportedIndustry}/data-holders/status";

            var expectedUrl = ReplacePublicHostName(url, STATUS_DOWNSTREAM_BASE_URL);

            var expected = GetExpectedDataHolderStatuses(expectedUrl);

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = xv,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader(expectedXV, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expected, response.Content);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("foo")]
        public async Task Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-holders/status";
            var expected = GetExpectedDataHolderStatuses(url);

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = SupportedVersion,
                IfNoneMatch = ifNoneMatch,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader(SupportedVersion, response.Headers, "x-v");

                // Assert - Check has any ETag
                Assert_HasHeader(null, response.Headers, "ETag");

                // Assert - Check json
                await Assert_HasContent_Json(expected, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-holders/status",
                XV = SupportedVersion,
                IfNoneMatch = null, // ie If-None-Match is not set
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-holders/status",
                XV = SupportedVersion,
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
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return supported version - x-min-v is ignored when > x-v
        [InlineData(LegacyEndpointVersion, UnsupportedEndpointVersion, LegacyEndpointVersion, HttpStatusCode.BadRequest, true, "", "non-bank-lending")] // Invalid industry. x-v is supported but industry is not supported for this version
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return supported version - x-v is supported and higher than x-min-v
        [InlineData(SupportedVersion, SupportedVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return supported version - x-v is supported equal to x-min-v
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "", "banking")] // Valid. Should return supported version as it supports 'Banking' industry.
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "", "energy")] // Valid. Should return supported version as it supports 'Energy' industry.
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "", "telco")] // Valid. Should return supported version as it supports 'Telco' industry.
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "", "non-bank-lending")] // Valid. Should return supported version as it supports 'Non-Bank Lending' industry.
        [InlineData(SupportedVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is supported but x-min-v is invalid (not a positive integer)
        [InlineData(UnsupportedEndpointVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v is invalid (not a positive integer)
        [InlineData(SupportedVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. Both x-v and x-min-v exceed maximum supported version
        [InlineData("foo", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo", LegacyEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData(UnsupportedEndpointVersion, null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is higher than a supported version
        [InlineData(SupportedVersion, LegacyEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "mining")] // Valid. Should return error code 400 - Bad Request - Invalid Path Parameter
        [InlineData("", "", LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Should return supported version.
        [InlineData("", UnsupportedEndpointVersion, LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Valid. empty header behaves like no header
        [InlineData(null, UnsupportedEndpointVersion, LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Valid. No x-v header, defaults to v1.
        [InlineData(LegacyEndpointVersion, UnsupportedEndpointVersion, LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Explicit v1 header to confirm API still supports v1

        // Ensure the x-v is optional until v1 is no longer supported
        [InlineData("", null, LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Invalid. x-v header is an empty string
        [InlineData(null, null, LegacyEndpointVersion, HttpStatusCode.OK, true, "")] // Invalid. x-v header is missing
        public async Task VersionHeaderValidation(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expectedError, string industry = "all")
        {
            // Arrange
            var request = new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-holders/status",
                XV = xv,
                XMinV = minXv,
            };

            // Act
            var response = await request.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedHttpStatusCode);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                if (isExpectedToBeSupported)
                {
                    // Assert - Check x-v returned header
                    Assert_HasHeader(expectedXv, response.Headers, "x-v");
                }
                else
                {
                    // Assert - Check error response
                    await Assert_HasContent_Json(expectedError, response.Content);
                }
            }
        }

        [Theory]
        [InlineData(LegacyEndpointVersion, "all")] // Explicit v1
        [InlineData(LegacyEndpointVersion, "banking")] // v1
        [InlineData(SupportedVersion, "all")] // v2
        [InlineData(SupportedVersion, "non-bank-lending")] // NBL v2
        [InlineData(SupportedVersion, "banking")]

        public async Task ValidateDataHolderStatusByIndustry(string? xv, string industry)
        {
            // Arrange
            var request = new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-holders/status",
                XV = xv,
            };

            // Act
            var response = await request.SendAsync();

            // Assert status code first
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert
            string expectedContent = string.Empty;

            if (string.IsNullOrEmpty(xv) || xv == LegacyEndpointVersion)
            {
                expectedContent = GetExpectedDataHolderStatusesLegacy(request.URL, industry);
            }
            else if (xv == SupportedVersion)
            {
                expectedContent = GetExpectedDataHolderStatuses(request.URL, industry);
            }

            await Assert_HasContent_Json(expectedContent, response.Content);
        }

        private static string GetExpectedDataHolderStatuses(string url, string industry = SupportedIndustry)
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var expectedDataHolderStatuses = new
            {
                data = dbContext.Participations.AsNoTracking()
                    .Include(p => p.Status)
                    .Where(p => p.ParticipationTypeId == ParticipationTypes.Dh)
                    .Where(p => p.Industry.IndustryTypeCode == industry || industry == "all")

                    .Select(p => new
                    {
                        legalEntityId = p.LegalEntityId,
                        status = p.Status.ParticipationStatusCode,
                    })
                    .OrderBy(p => p.legalEntityId)
                    .ToList(),
                links = new
                {
                    self = url,
                },
                meta = new object(),
            };

            return JsonConvert.SerializeObject(expectedDataHolderStatuses);
        }

        private static string GetExpectedDataHolderStatusesLegacy(string url, string industry = SupportedIndustry)
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var expectedDataHolderStatuses = new
            {
                data = dbContext.Participations.AsNoTracking()
                    .Include(p => p.Status)
                    .Where(p => p.ParticipationTypeId == ParticipationTypes.Dh)
                    .Where(p => p.Industry.IndustryTypeCode == industry || industry == "all")
                    .Where(p => p.Industry.IndustryTypeCode != "non-bank-lending")
                    .Select(p => new
                    {
                        legalEntityId = p.LegalEntityId,
                        status = p.Status.ParticipationStatusCode,
                    })
                    .OrderBy(p => p.legalEntityId)
                    .ToList(),
                links = new
                {
                    self = url,
                },
                meta = new object(),
            };

            return JsonConvert.SerializeObject(expectedDataHolderStatuses);
        }
    }
}
