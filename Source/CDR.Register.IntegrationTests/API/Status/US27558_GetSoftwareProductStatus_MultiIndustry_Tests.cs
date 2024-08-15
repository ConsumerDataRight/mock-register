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
using Xunit.Abstractions;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Status
{
    /// <summary>
    /// Integration tests for GetSoftwareProductStatus v2 endpoints
    /// </summary>   
    public class US27558_GetSoftwareProductStatus_MultiIndustry_Tests : BaseTest
    {
        public US27558_GetSoftwareProductStatus_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }
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

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData("2", "2")]
        public async Task AC01_CTS_URL_Get_WithXV_ShouldRespondWith_200OK_SoftwareProducts(string? XV, string expectedXV)
        {
            // Arrange 
            var url = $"{GenerateDynamicCtsUrl(STATUS_DOWNSTREAM_BASE_URL)}/cdr-register/v1/all/data-recipients/brands/software-products/status";
            var expectedSoftwareProductsStatus = GetExpectedSoftwareProductStatus(ReplacePublicHostName(url, STATUS_DOWNSTREAM_BASE_URL));

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
        [InlineData("2",    "3",    "2",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v2 - x-min-v is ignored when > x-v
        [InlineData("2",    "1",    "2",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v2 - x-v is supported and higher than x-min-v 
        [InlineData("2",    "2",    "2",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v2 - x-v is supported equal to x-min-v 
        [InlineData("3",    "2",    "2",    HttpStatusCode.OK,              true,  "")]                             //Valid. Should return v2 - x-v is NOT supported and x-min-v is supported Z       
        [InlineData("3",    "foo",  "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is supported but x-min-v is invalid (not a positive integer) 
        [InlineData("4",    "foo",  "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is not supported and x-min-v is invalid (not a positive integer) 
        [InlineData("3",    "0",    "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is not supported and x-min-v invalid
        [InlineData("3",    "3",    "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. Both x-v and x-min-v exceed supported version of 2
        [InlineData("1",    null,   "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. x-v is an obsolete version        
        [InlineData("foo",  null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0",    null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo",  "2",    "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1",   null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_INVALID_VERSION_ERROR)] //Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData("3",    null,   "N/A",  HttpStatusCode.NotAcceptable,   false, EXPECTED_UNSUPPORTED_ERROR)]     //Unsupported. x-v is higher than supported version of 2
        [InlineData("",     null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_MISSING_X_V_ERROR)]     //Invalid. x-v header is an empty string
        [InlineData(null,   null,   "N/A",  HttpStatusCode.BadRequest,      false, EXPECTED_MISSING_X_V_ERROR)]      //Invalid. x-v header is missing      

        public async Task ACXX_VersionHeaderValidation(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expecetdError)
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
