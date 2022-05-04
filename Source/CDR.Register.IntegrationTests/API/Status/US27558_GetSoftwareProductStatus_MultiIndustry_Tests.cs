using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Status
{
    /// <summary>
    /// Integration tests for GetSoftwareProductStatus v2 endpoints
    /// </summary>   
    public class US27558_GetSoftwareProductStatus_MultiIndustry_Tests : BaseTest
    {
        private static string GetExpectedSoftwareProductStatus(string url)
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var expectedSoftwareProductsStatus = new
            {
                data = dbContext.SoftwareProducts.AsNoTracking<Repository.Entities.SoftwareProduct>()
                    .Include(sp => sp.Status)
                    .Select(sp => new
                    {
                        softwareProductId = sp.SoftwareProductId,
                        status = sp.Status.SoftwareProductStatusCode
                    })
                    .OrderBy(sp => sp.softwareProductId.ToString())
                    .ToList(),
                links = new { 
                    self = url
                },
                meta = new object()
            };

            return JsonConvert.SerializeObject(expectedSoftwareProductsStatus);
        }

        [Theory]
        [InlineData("2", "2")]
        public async Task AC01_Get_WithXV_ShouldRespondWith_200OK_SoftwareProducts(string? XV, string expectedXV)
        {
            // Arrange 
            var url = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status";
            var expectedSoftwareProductsStatus = GetExpectedSoftwareProductStatus(url);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = XV
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
                await Assert_HasContent_Json(expectedSoftwareProductsStatus, response.Content);
            }
        }

        //[Theory]
        //[InlineData(null)] // DF: this will no longer throw an error.
        //public async Task AC02_Get_WithMissingXV_ShouldRespondWith_400BadRequest(string? XV)
        //{
        //    // Act
        //    var response = await new Infrastructure.API
        //    {
        //        HttpMethod = HttpMethod.Get,
        //        URL = $"{TLS_BaseURL}/cdr-register/v1/data-recipients/brands/software-products/status",
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
        public async Task AC07_Get_WithInvalidXV_ShouldRespondWith_400BadRequest(string? XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status",
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
        [InlineData("3")]
        [InlineData("99")]
        public async Task AC03_Get_UnsupportedXV_ShouldRespondWith_406NotAcceptable(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status",
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
                    ""detail"": ""minimum version: 1, maximum version: 2"",
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
            var url = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status";
            var expectedSoftwareProductsStatus = GetExpectedSoftwareProductStatus(url);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = url,
                XV = "2",
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
                Assert_HasHeader("2", response.Headers, "x-v");

                // Assert - Check has any ETag
                Assert_HasHeader(null, response.Headers, "ETag");

                // Assert - Check json
                await Assert_HasContent_Json(expectedSoftwareProductsStatus, response.Content);
            }
        }

        [Fact]
        public async Task AC05_Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status",
                XV = "2",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/all/data-recipients/brands/software-products/status",
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
        [InlineData("99", "2")]
        public async Task ACXX_Get_WithMinXV_ShouldRespondWith_200OK(string xv, string minXv)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
        [InlineData("2", "2")]
        [InlineData("2", "3")]
        public async Task ACXX_Get_WithMinXVGreaterThanOrEqualToXV_ShouldBeIgnored_200OK(string xv, string minXv)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
                Assert_HasHeader("1", response.Headers, "x-v");
            }
        }

        [Theory]
        [InlineData("99", "foo")]
        [InlineData("99", "0")]
        [InlineData("99", "-1")]
        public async Task ACXX_Get_WithInvalidMinXV_ShouldReturnInvalidVersionError_400BadRequest(string xv, string minXv)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
        [InlineData("99", "10")]
        public async Task ACXX_Get_WithUnsupportedMinXV_ShouldReturnUnsupportedVersionError_406NotAccepted(string xv, string minXv)
        {
            // Arrange
            var expectedContent = EXPECTEDCONTENT_UNSUPPORTEDXV.Replace("#{minVersion}", "1").Replace("#{maxVersion}", "2");

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
