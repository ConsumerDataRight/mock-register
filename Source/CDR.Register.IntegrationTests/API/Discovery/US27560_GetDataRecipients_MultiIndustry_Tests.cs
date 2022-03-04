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
using System;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class US27560_GetDataRecipients_MultiIndustry_Tests : BaseTest
    {
        // Get expected data recipients
        private static string GetExpectedDataRecipients()
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
                        .Where(participation => participation.ParticipationTypeId == ParticipationTypeEnum.Dr)
                        .OrderBy(participation => participation.LegalEntityId)
                        .Select(participation => new
                        {
                            legalEntityId = participation.LegalEntityId,
                            legalEntityName = participation.LegalEntity.LegalEntityName,
                            accreditationNumber = participation.LegalEntity.AccreditationNumber,
                            accreditationLevel = participation.LegalEntity.AccreditationLevel.AccreditationLevelCode,
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

                return JsonConvert.SerializeObject(expectedDataRecipients);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting expected data recipients - {ex.Message}");
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task AC01_Get_WithXV_ShouldRespondWith_200OK_DataRecipients(int XV)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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
        [InlineData(null)]
        public async Task AC02_Get_WithMissingXV_ShouldRespondWith_400BadRequest(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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
                    ""code"": ""urn:au-cds:error:cds-all:Header/Missing"",
                    ""title"": ""Missing Required Header"",
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
        public async Task ACX02_Get_WithInvalidXV_ShouldRespondWith_400BadRequest(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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

        [Theory]
        [InlineData("2")]
        [InlineData("99")]
        public async Task AC03_Get_WithUnsupportedXV_ShouldRespondWith_406NotAcceptable(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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
                    ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
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
        [InlineData(null)] // AC04
        [InlineData("foo")] // AC06
        public async Task AC04_AC06_Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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
        public async Task AC05_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
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
