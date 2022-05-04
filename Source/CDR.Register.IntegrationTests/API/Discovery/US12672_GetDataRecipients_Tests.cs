using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Microsoft.Extensions.Configuration;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class US12672_GetDataRecipients_Tests : BaseTest
    {
        // Get expected data recipients (for Banking)
        private static string GetExpectedResponse_Banking(int? XV)
        {
            // If XV header is omitted then we should expect API to treat XV as 1
            if (XV == null)
            {
                XV = 1;
            }

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
                        accreditationNumber = XV >= 2 ? participation.LegalEntity.AccreditationNumber : null,
                        legalEntityId = participation.LegalEntityId,
                        legalEntityName = participation.LegalEntity.LegalEntityName,
                        industry = "banking", // banking is always returned for legacy API
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
                        lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                    })
                    .ToList()
            };

            return JsonConvert.SerializeObject(expectedDataRecipients,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });
        }

        [Theory]
        [InlineData(null, "1")] // AC02
        [InlineData(2, "2")] // AC01 // AC05 was changed in AC, it's now same as AC01
        public async Task AC01_AC02_AC05_Get_ShouldRespondWith_200OK_DataRecipientsStatus(int? XV, string expectedXV)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedResponse_Banking(XV);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV?.ToString()
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
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Theory]
        [InlineData("foo", "2")]
        // [InlineData("foo", "")] // XV not set  - No longer needed US30562    
        public async Task AC03_Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest_ErrorResponse(string industry, string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients",
                XV = XV
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader("2", response.Headers, "x-v");

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

        [Theory]
        [InlineData("4")] // AC04
        //[InlineData("1")] // AC10
        public async Task AC04_AC10_Get_UnsupportedXV_ShouldRespondWith_406NotAcceptable_ErrorResponse(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                // Assert_HasHeader("2", response.Headers, "x-v");  // No longer needed US31280

                // Assert - Check error response
                var expectedContent = @"
                {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                    ""title"": ""Unsupported Version"",
                    ""detail"": ""minimum version: 1, maximum version: 3"",
                    ""meta"": {}
                    }
                ]
                }";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null, null)] // AC06
        [InlineData(null, 2)] // AC06
        [InlineData("foo", null)] // AC08
        [InlineData("foo", 2)] // AC08
        public async Task AC06_AC08_Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch, int? XV)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedResponse_Banking(XV);
            var expectedXV = XV.HasValue ? XV.Value : 1;

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV?.ToString(),
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
                Assert_HasHeader(expectedXV.ToString(), response.Headers, "x-v");

                // Assert - Check has any ETag
                Assert_HasHeader(null, response.Headers, "ETag");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(2)]
        public async Task AC07_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag(int? XV)
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV?.ToString(),
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV?.ToString(),
                IfNoneMatch = expectedETag.Trim('"'),
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotModified);

                // Assert - Check ETag matches expected ETag
                Assert_HasHeader(expectedETag, response.Headers, "ETag");

                // Assert - No content
                (await response.Content.ReadAsStringAsync()).Should().BeNullOrEmpty();
            }
        }

        [Theory]
        [InlineData("foo")]
        public async Task AC09_Get_InvalidXV_ShouldRespondWith_400BadRequest_ErrorResponse(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                var expectedContent = @"
                {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/InvalidVersion"",
                    ""title"": ""Invalid Version"",
                    ""detail"": """",
                    ""meta"": {}
                    }
                ]
                }";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
    }
}
