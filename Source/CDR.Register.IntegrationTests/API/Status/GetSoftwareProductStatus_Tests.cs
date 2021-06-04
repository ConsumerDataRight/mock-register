using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Status
{
    /// <summary>
    /// Integration tests for GetSoftwareProductStatus.
    /// </summary>   
    public class GetSoftwareProductStatus_Tests : BaseTest
    {
        static private string GetExpectedSoftwareProductStatus()
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlite(SQLITECONNECTIONSTRING).Options);

            var expectedSoftwareProductsStatus = new
            {
                softwareProducts = dbContext.SoftwareProducts.AsNoTracking<Repository.Entities.SoftwareProduct>()
                    .Include(sp => sp.Status)
                    .OrderBy(sp => sp.SoftwareProductId)
                    .Select(sp => new
                    {
                        softwareProductId = sp.SoftwareProductId,
                        softwareProductStatus = sp.Status.SoftwareProductStatusCode
                    })
                    .ToList()
            };

            return JsonConvert.SerializeObject(expectedSoftwareProductsStatus);
        }

        [Theory]
        [InlineData("1", "1")] 
        [InlineData(null, "1")]
        public async Task Get_WithXV_ShouldRespondWith_200OK_SoftwareProducts(string? XV, string expectedXV)
        {
            // Arrange 
            var expectedSoftwareProductsStatus = GetExpectedSoftwareProductStatus();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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

        [Theory]
        [InlineData("foo")]
        public async Task Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest_ErrorResponse(string industry)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients/brands/software-products/status",
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
        [InlineData("2")]
        public async Task Get_UnsupportedXV_ShouldRespondWith_406NotAcceptable_ErrorResponse(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
            var expectedSoftwareProductsStatus = GetExpectedSoftwareProductStatus();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
                await Assert_HasContent_Json(expectedSoftwareProductsStatus, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/brands/software-products/status",
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
