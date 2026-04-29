using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
    /// Integration tests for GetDataRecipientStatus V3 endpoints.
    /// </summary>
    public class GetDataRecipientStatusXV3_MultiIndustry_Tests : BaseTest
    {
        /// <summary>
        /// The supported industry in V3.
        /// </summary>
        private const string SupportedIndustry = "all";

        /// <summary>
        /// The supported version - 3.
        /// </summary>
        private const string SupportedVersion = "3";

        /// <summary>
        /// The previous endpoint version which is still supported.
        /// </summary>
        private const string LegacyEndpointVersion = "2";

        /// <summary>
        /// The future (not implemented) endpoint version, which isn't supported.
        /// </summary>
        private const string UnsupportedEndpointVersion = "4";

        public GetDataRecipientStatusXV3_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture)
            : base(outputHelper, testFixture)
        {
        }

        [Theory]
        [InlineData(SupportedVersion, SupportedVersion)]
        public async Task Get_ShouldRespondWith_200OK_DataRecipientsStatus(string? xv, string expectedXV)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-recipients/status";
            var expectedDataRecipientStatus = GetExpectedDataRecipientsStatus(url);

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
                await Assert_HasContent_Json(expectedDataRecipientStatus, response.Content);
            }
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(SupportedVersion, SupportedVersion)]
        public async Task Get_WithDynamicPaths_ShouldRespondWith_200OK_DataRecipientsStatus(string? xv, string expectedXV)
        {
            // Arrange
            var url = $"{GenerateDynamicCtsUrl(STATUS_DOWNSTREAM_BASE_URL)}/cdr-register/v1/{SupportedIndustry}/data-recipients/status";

            var expectedUrl = ReplacePublicHostName(url, STATUS_DOWNSTREAM_BASE_URL);

            var expectedDataRecipientStatus = GetExpectedDataRecipientsStatus(expectedUrl);

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
                await Assert_HasContent_Json(expectedDataRecipientStatus, response.Content);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("foo")]
        public async Task Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-recipients/status";
            var expectedDataRecipientsStatus = GetExpectedDataRecipientsStatus(url);

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
                await Assert_HasContent_Json(expectedDataRecipientsStatus, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-recipients/status",
                XV = SupportedVersion,
                IfNoneMatch = null, // ie If-None-Match is not set
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{SupportedIndustry}/data-recipients/status",
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
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v3 - x-min-v is ignored when > x-v
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v3 - x-min-v is ignored when > x-v
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, SupportedVersion, HttpStatusCode.BadRequest, true, "", "banking")] // Invalid industry. x-v is supported but industry is not supported for v3
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.BadRequest, true, "", "Energy")] // Invalid industry. x-v is supported but industry is not supported for v3
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, SupportedVersion, HttpStatusCode.BadRequest, true, "", "TELCO")] // Invalid industry. x-v is supported but industry is not supported for v3
        [InlineData(SupportedVersion, "1", SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v3 - x-v is supported and higher than x-min-v
        [InlineData(SupportedVersion, SupportedVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v2 - x-v is supported equal to x-min-v
        [InlineData(UnsupportedEndpointVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.BadRequest, true, "", "banking")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported. BadRequest as v3 doesn't support 'Banking' industry
        [InlineData(UnsupportedEndpointVersion, SupportedVersion, SupportedVersion, HttpStatusCode.OK, true, "", "all")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported. BadRequest as v3 doesn't support 'Banking' industry
        [InlineData(SupportedVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is supported but x-min-v is invalid (not a positive integer)
        [InlineData(UnsupportedEndpointVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v is invalid (not a positive integer)
        [InlineData(SupportedVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. Both x-v and x-min-v exceed maximum supported version
        [InlineData("1", null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is an obsolete version
        [InlineData("foo", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo", LegacyEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData(UnsupportedEndpointVersion, null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. Both x-v and x-min-v exceed maximum supported version
        [InlineData("", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is an empty string
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is missing

        public async Task VersionHeaderValidation(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expecetdError, string industry = "all")
        {
            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients/status",
                XV = xv,
                XMinV = minXv,
            }.SendAsync();

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
                    await Assert_HasContent_Json(expecetdError, response.Content);
                }
            }
        }

        private static string GetExpectedDataRecipientsStatus(string url)
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var expectedDataRecipientsStatus = new
            {
                data = dbContext.Participations.AsNoTracking<Repository.Entities.Participation>()
                    .Include(p => p.Status)
                    .Where(p => p.ParticipationTypeId == ParticipationTypes.Dr)
                    .Select(p => new
                    {
                        legalEntityId = p.LegalEntityId,
                        status = p.Status.ParticipationStatusCode,
                    })
                    .OrderBy(p => p.legalEntityId.ToString())
                    .ToList(),
                links = new
                {
                    self = url,
                },
                meta = new object(),
            };

            return JsonConvert.SerializeObject(expectedDataRecipientsStatus);
        }
    }
}
