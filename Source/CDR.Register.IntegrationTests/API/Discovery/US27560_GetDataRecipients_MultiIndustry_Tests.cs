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
using Xunit.Abstractions;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class US27560_GetDataRecipients_MultiIndustry_Tests : BaseTest
    {
        public US27560_GetDataRecipients_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }

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
                                    status = softwareProduct.Status.SoftwareProductStatusCode
                                }),
                                status = brand.BrandStatus.BrandStatusCode,
                            }),
                            status = participation.Status.ParticipationStatusCode,
                            lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        })
                        .ToList(),
                    // DF: these are new properties that need to be included in the Get Data Recipients payload.
                    links = new { 
                        self = url
                    },
                    meta = new object()
                };

                string result = JsonConvert.SerializeObject(expectedDataRecipients);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting expected data recipients - {ex.Message}");
            }
        }

        [Theory]
        [InlineData(3, "all")]
        [InlineData(3, "banking")]
        [InlineData(3, "energy")]
        [InlineData(3, "telco")]
        public async Task AC01_Get_WithXV_ShouldRespondWith_200OK_DataRecipients(int XV, string industry)
        {
            // Arrange 
            var url = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(url);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = XV.ToString()
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // // Assert - Check XV
                Assert_HasHeader(XV.ToString(), response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(3, "all")]
        [InlineData(3, "banking")]
        [InlineData(3, "energy")]
        [InlineData(3, "telco")]
        public async Task AC01_CTS_URL_Get_WithXV_ShouldRespondWith_200OK_DataRecipients(int XV, string industry)
        {
            // Arrange 
            var url = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL)}/cdr-register/v1/{industry}/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(ReplacePublicHostName(url, DISCOVERY_DOWNSTREAM_BASE_URL));

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = XV.ToString()
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // // Assert - Check XV
                Assert_HasHeader(XV.ToString(), response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] // AC04
        [InlineData("foo")] // AC06
        public async Task AC04_AC06_Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange 
            var url = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients";
            var expectedDataRecipients = GetExpectedDataRecipients(url);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = "3",
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
                Assert_HasHeader("3", response.Headers, "x-v");

                // Assert - Check has any ETag
                Assert_HasHeader(null, response.Headers, "ETag");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Fact]
        public async Task AC05_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
                XV = "3",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
                XV = "3",
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
        [InlineData("3",    "4",    "3",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v3 - x-min-v is ignored when > x-v
        [InlineData("3",    "2",    "3",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v3 - x-v is supported and higher than x-min-v 
        [InlineData("3",    "3",    "3",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v3 - x-v is supported equal to x-min-v 
        [InlineData("4",    "3",    "3",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v3 - x-v is NOT supported and x-min-v is supported        
        [InlineData("3",    "foo",  "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is supported but x-min-v is invalid (not a positive integer) 
        [InlineData("99",   "foo",  "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is not supported and x-min-v is invalid (not a positive integer) 
        [InlineData("4",    "0",    "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //invalid. x-v is not supported and x-min-v invalid
        [InlineData("4",    "4",    "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. Both x-v and x-min-v exceed supported version of 3
        [InlineData("1",    null,   "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. x-v is an obsolete version
        [InlineData("2",    null,   "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. x-v is an obsolete version        
        [InlineData("foo",  null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0",    null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo",  "3",    "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1",   null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData("4",    null,   "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. x-v is higher than supported version of 3
        [InlineData("",     null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_MISSING_X_V_ERROR)]     //Invalid. x-v header is an empty string
        [InlineData(null,   null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_MISSING_X_V_ERROR)]     //Invalid. x-v header is missing

        public async Task ACX01_VersionHeaderValidation(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expecetdError)
        {

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = xv,
                XMinV = minXv
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
    }
}
