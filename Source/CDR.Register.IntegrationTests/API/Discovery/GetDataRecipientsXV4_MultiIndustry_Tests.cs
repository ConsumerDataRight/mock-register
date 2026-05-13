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

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class GetDataRecipientsXV4_MultiIndustry_Tests : BaseTest
    {
        /// <summary>
        /// The current endpoint version under test.
        /// </summary>
        private const string SupportedVersion = "4";

        /// <summary>
        /// The previous endpoint version which is still supported.
        /// </summary>
        private const string LegacyEndpointVersion = "3";

        /// <summary>
        /// The future (not implemented) endpoint version, which isn't supported.
        /// </summary>
        private const string UnsupportedEndpointVersion = "5";

        public GetDataRecipientsXV4_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture)
            : base(outputHelper, testFixture)
        {
        }

        [Theory]
        [InlineData(SupportedVersion, "all")]
        public async Task Get_WithXV_ShouldRespondWith_200OK_DataRecipients(string xv, string industry)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(url);

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

                // // Assert - Check XV
                Assert_HasHeader(xv, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(SupportedVersion, "all")]
        public async Task CTS_URL_Get_WithXV_ShouldRespondWith_200OK_DataRecipients(string xv, string industry)
        {
            // Arrange
            var url = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL)}/cdr-register/v1/{industry}/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(ReplacePublicHostName(url, DISCOVERY_DOWNSTREAM_BASE_URL));

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

                // // Assert - Check XV
                Assert_HasHeader(xv, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // AC04
        [InlineData("foo")] // AC06
        public async Task Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange
            var url = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(url);

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
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
                XV = SupportedVersion,
                IfNoneMatch = null, // ie If-None-Match is not set
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
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
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v4 - x-min-v is ignored when > x-v
        [InlineData(SupportedVersion, LegacyEndpointVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v4 - x-v is supported and higher than x-min-v
        [InlineData(SupportedVersion, SupportedVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v4 - x-v is supported equal to x-min-v
        [InlineData(SupportedVersion, SupportedVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "banking")] // Invalid. Industry other than 'All'
        [InlineData(SupportedVersion, LegacyEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "ENERGY")] // Invalid.Industry other than 'All'
        [InlineData(SupportedVersion, LegacyEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "telCO")] // Invalid.Industry other than 'All'
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "non-bank-lending")] // Invalid. Industry other than 'All'
        [InlineData(SupportedVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "Mining")] // Invalid. Industry other than 'All'
        [InlineData(LegacyEndpointVersion, "2", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALIDFIELD_INDUSTRY, "non-bank-lending")] // Invalid. Industry other than 'All'
        [InlineData(UnsupportedEndpointVersion, SupportedVersion, SupportedVersion, HttpStatusCode.OK, true, "")] // Valid. Should return v4 - x-v is NOT supported and x-min-v is supported
        [InlineData(SupportedVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is supported but x-min-v is invalid (not a positive integer)
        [InlineData("99", "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v is invalid (not a positive integer)
        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // invalid. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. Both x-v and x-min-v exceed supported version of 3
        [InlineData("1", null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is an obsolete version
        [InlineData("2", null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is an obsolete version
        [InlineData("foo", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo", SupportedVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData(UnsupportedEndpointVersion, null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is higher than supported version of 3
        [InlineData("", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is an empty string
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is missing

        public async Task VersionHeaderValidation(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expecetdError, string industry = "all")
        {
            // Act
            var response = await new Infrastructure.Api
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients",
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

        // Get expected data recipients
        private static string GetExpectedDataRecipients(string url)
        {
            try
            {
                using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

                var expectedDataRecipients = new
                {
                    data = dbContext.Participations.AsNoTracking()
                        .Include(participation => participation.Status)
                        .Include(participation => participation.Industry)
                        .Include(participation => participation.LegalEntity)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.BrandStatus)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.SoftwareProducts)
                        .ThenInclude(softwareProduct => softwareProduct.Status)
                        .Where(participation => participation.ParticipationTypeId == ParticipationTypes.Dr)
                        .OrderBy(participation => participation.LegalEntityId)
                        .Select(participation => new
                        {
                            legalEntityId = participation.LegalEntityId,
                            legalEntityName = participation.LegalEntity.LegalEntityName,
                            accreditationNumber = participation.LegalEntity.AccreditationNumber,
                            accreditationLevel = participation.LegalEntity.AccreditationLevel.AccreditationLevelCode.ToUpper(), // DF: accreditation level should be uppercase.
                            logoUri = participation.LegalEntity.LogoUri,
                            dataRecipientBrands = participation.Brands.OrderBy(b => b.BrandId).Select(brand => new
                            {
                                dataRecipientBrandId = brand.BrandId,
                                brandName = brand.BrandName,
                                logoUri = brand.LogoUri,
                                softwareProducts = brand.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId.ToString()).Select(softwareProduct => new
                                {
                                    softwareProductId = softwareProduct.SoftwareProductId,
                                    softwareProductName = softwareProduct.SoftwareProductName,
                                    softwareProductDescription = softwareProduct.SoftwareProductDescription,
                                    logoUri = softwareProduct.LogoUri,
                                    status = softwareProduct.Status.SoftwareProductStatusCode,
                                }),
                                status = brand.BrandStatus.BrandStatusCode,
                            }),
                            status = participation.Status.ParticipationStatusCode,
                            lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        })
                        .ToList(),

                    // DF: these are new properties that need to be included in the Get Data Recipients payload.
                    links = new
                    {
                        self = url,
                    },
                    meta = new object(),
                };

                string result = JsonConvert.SerializeObject(expectedDataRecipients);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting expected data recipients - {ex.Message}");
            }
        }
    }
}
