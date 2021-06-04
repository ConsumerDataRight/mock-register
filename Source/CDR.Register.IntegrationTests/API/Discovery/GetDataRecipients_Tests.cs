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

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class GetDataRecipients_Tests : BaseTest
    {
        static private string GetExpectedDataRecipients(int? XV)
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlite(SQLITECONNECTIONSTRING).Options);

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
                    .Where(participation => participation.ParticipationTypeId == ParticipationTypeEnum.Dr)
                    .OrderBy(participation => participation.LegalEntityId)
                    .Select(participation => new
                    {
                        legalEntityId = participation.LegalEntityId,
                        legalEntityName = participation.LegalEntity.LegalEntityName,
                        accreditationNumber = XV >= 2 ? participation.LegalEntity.AccreditationNumber : null,
                        industry = participation.Industry.IndustryTypeCode,
                        logoUri = participation.LegalEntity.LogoUri,
                        status = participation.Status.ParticipationStatusCode,
                        dataRecipientBrands = participation.Brands.OrderBy(b => b.BrandId).Select(brand => new
                        {
                            dataRecipientBrandId = brand.BrandId,
                            brandName = brand.BrandName,
                            logoUri = brand.LogoUri,
                            softwareProducts = brand.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).Select(softwareProduct => new
                            {
                                softwareProductId = softwareProduct.SoftwareProductId,
                                softwareProductName = softwareProduct.SoftwareProductName,
                                softwareProductDescription = softwareProduct.SoftwareProductDescription,
                                logoUri = softwareProduct.LogoUri,
                                status = softwareProduct.Status.SoftwareProductStatusCode
                            }),
                            status = brand.BrandStatus.BrandStatusCode,
                        }),
                        lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                    })
                    .ToList()
            };

            return JsonConvert.SerializeObject(expectedDataRecipients,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    // DateTimeZoneHandling = DateTimeZoneHandling.Utc, 
                    Formatting = Formatting.Indented
                });
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData(1, "1")] 
        [InlineData(2, "2")] 
        public async Task Get_ShouldRespondWith_200OK_DataRecipientsStatus(int? XV, string expectedXV)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients(XV);

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
        [InlineData("foo")]
        public async Task Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest_ErrorResponse(string industry)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients",
                XV = "1"
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

        [Theory]
        [InlineData("foo")]
        [InlineData("3")]
        public async Task Get_UnsupportedXV_ShouldRespondWith_406NotAcceptable_ErrorResponse(string XV)
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

        [Theory]
        [InlineData(null)] 
        [InlineData("foo")] 
        public async Task Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients(1);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = "1",
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
                Assert_HasHeader("1", response.Headers, "x-v");

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
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
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
