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
                            lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                        })
                        .ToList(),
                    // DF: these are new properties that need to be included in the Get Data Recipients payload.
                    links = new { 
                        self = url
                    },
                    meta = new object()
                };

                return JsonConvert.SerializeObject(expectedDataRecipients);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting expected data recipients - {ex.Message}");
            }
        }

        [Theory]
        [InlineData(3)]
        public async Task AC01_Get_WithXV_ShouldRespondWith_200OK_DataRecipients(int XV)
        {
            // Arrange 
            var url = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients";
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

        // DF: this check is no longer possible.
        //[Theory]
        //[InlineData(null)]
        //public async Task AC02_Get_WithMissingXV_ShouldRespondWith_400BadRequest(string XV)
        //{
        //    // Act
        //    var response = await new Infrastructure.API
        //    {
        //        HttpMethod = HttpMethod.Get,
        //        URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients",
        //        XV = XV
        //    }.SendAsync();

        //    // Assert
        //    using (new AssertionScope())
        //    {
        //        // Assert - Check status code
        //        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        //        // Assert - Check content type
        //        Assert_HasContentType_ApplicationJson(response.Content);

        //        // Assert - Check error response
        //        var expectedContent = @"
        //        {
        //        ""errors"": [
        //            {
        //            ""code"": ""urn:au-cds:error:cds-all:Header/Missing"",
        //            ""title"": ""Missing Required Header"",
        //            ""detail"": """",
        //            ""meta"": {}
        //            }
        //        ]
        //        }";
        //        await Assert_HasContent_Json(expectedContent, response.Content);
        //    }
        //}

        [Theory]
        [InlineData("foo")]
        public async Task ACX02_Get_WithInvalidXV_ShouldRespondWith_400BadRequest(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
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
        [InlineData("0", HttpStatusCode.BadRequest, EXPECTEDCONTENT_INVALIDXV)]
        [InlineData("99", HttpStatusCode.NotAcceptable, EXPECTEDCONTENT_UNSUPPORTEDXV)]
        public async Task AC03_Get_WithUnsupportedXV_ShouldRespondWith_406NotAcceptable(string XV, HttpStatusCode expectedStatusCode, string expectedContent)
        {
            // Arrange.
            expectedContent = expectedContent.Replace("#{minVersion}", "1").Replace("#{maxVersion}", "3");

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients",
                XV = XV
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                await Assert_HasContent_Json(expectedContent, response.Content);
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
        [InlineData("99", "3")]
        [InlineData("3", "3")]
        public async Task AC12_Get_WithMinXV_ShouldRespondWith_200OK(string xv, string minXv)
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
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                Assert_HasHeader("3", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("2", "2")]
        [InlineData("2", "3")]
        public async Task ACXX_Get_WithMinXVGreaterThanOrEqualToXV_ShouldBeIgnored_200OK(string xv, string minXv)
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
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check x-v returned header
                Assert_HasHeader("1", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("99", "foo")]
        [InlineData("99", "0")]
        [InlineData("99", "-1")]
        public async Task AC14_Get_WithInvalidMinXV_ShouldReturnInvalidVersionError_400BadRequest(string xv, string minXv)
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
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                await Assert_HasContent_Json(EXPECTEDCONTENT_INVALIDXV, response.Content);
            }
        }

        [Theory]
        [InlineData("2", "foo")]
        public async Task AC15_Get_WithValidXvAndInvalidMinXV_ShouldBeIgnored_200OK(string xv, string minXv)
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
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check x-v returned header
                Assert_HasHeader("2", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("99", "10")]
        public async Task ACXX_Get_WithUnsupportedMinXV_ShouldReturnUnsupportedVersionError_406NotAccepted(string xv, string minXv)
        {
            // Arrange
            var expectedContent = EXPECTEDCONTENT_UNSUPPORTEDXV.Replace("#{minVersion}", "1").Replace("#{maxVersion}", "3");

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
                response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }
    }
}
