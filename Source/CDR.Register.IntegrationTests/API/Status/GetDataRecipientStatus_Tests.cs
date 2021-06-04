using System;
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

namespace CDR.Register.IntegrationTests.API.Status
{
    /// <summary>
    /// Integration tests for GetDataRecipientStatus.
    /// </summary>
    public class GetDataRecipientStatus_Tests : BaseTest
    {
        static private string GetExpectedDataRecipientsStatus()
        {
            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlite(SQLITECONNECTIONSTRING).Options);

            var expectedDataRecipientsStatus = new
            {
                dataRecipients = dbContext.Participations.AsNoTracking<Repository.Entities.Participation>()
                    .Include(p => p.Status)
                    .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                    .OrderBy(p => p.ParticipationId)
                    .Select(p => new
                    {
                        dataRecipientId = p.LegalEntityId,
                        dataRecipientStatus = p.Status.ParticipationStatusCode
                    })
                    .ToList()
            };

            return JsonConvert.SerializeObject(expectedDataRecipientsStatus);
        }

        [Theory]
        [InlineData("1", "1")] 
        [InlineData(null, "1")] 
        public async Task Get_ShouldRespondWith_200OK_DataRecipientsStatus(string? XV, string expectedXV)
        {
            // Arrange 
            var expectedDataRecipientStatus = GetExpectedDataRecipientsStatus();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/status",
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
                await Assert_HasContent_Json(expectedDataRecipientStatus, response.Content);
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
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients/status",
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
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/status",
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
            var expectedDataRecipientsStatus = GetExpectedDataRecipientsStatus();

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/status",
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
                await Assert_HasContent_Json(expectedDataRecipientsStatus, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/status",
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients/status",
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
