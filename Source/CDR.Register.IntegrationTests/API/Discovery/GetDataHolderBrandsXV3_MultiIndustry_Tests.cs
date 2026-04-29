using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Infrastructure;
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
    /// Integration tests for GetDataHolderBrands V3.
    /// </summary>
    /// <remarks>
    /// This is similar to <see cref="US27562_GetDataHolderBrandsXV2_MultiIndustry_Tests"/> except
    /// that for <c>x-v:3</c> the additional <c>non-bank-lending</c> industry should be supported.
    /// </remarks>
    public class GetDataHolderBrandsXV3_MultiIndustry_Tests(ITestOutputHelper outputHelper, TestFixture testFixture)
        : BaseTest(outputHelper, testFixture)
    {
        private const string BrandId = "20C0864B-CEEF-4DE0-8944-EB0962F825EB";
        private const string SoftwareProductId = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        /// <summary>
        /// The current endpoint version under test.
        /// </summary>
        private const string EndpointVersion = "3";

        /// <summary>
        /// The previous endpoint version which is still supported.
        /// </summary>
        private const string LegacyEndpointVersion = "2";

        /// <summary>
        /// The future (not implemented) endpoint version, which isn't supported.
        /// </summary>
        private const string UnsupportedEndpointVersion = "4";

        /// <summary>
        /// Actions that need to be performed before the request, such as setting status of Brand or Software Product.
        /// </summary>
        private delegate void BeforeRequest();

        /// <summary>
        /// Actions that need to be performed after the request, such as restoring status of Brand or Software Product.
        /// </summary>
        private delegate void AfterRequest();

        // Participation/Brand/SoftwareProduct Ids
        private static string ParticipationId => GetParticipationId(BrandId); // lookup

        /// <summary>
        /// The request with no additional query parameters should return the expected results using the defaults (first page, 25 records).
        /// </summary>
        /// <param name="industry">The industry to request.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)]
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithNoQueryString_ShouldRespondWith_200OK_First25RecordsAsync(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_QueryParameters_RespondWithExpectedStatusAndBody(null, null, null, industry, expectedStatusCode);
        }

        /// <summary>
        /// The request with CTS paths and no additional query parameters should return the expected results using the defaults (first page, 25 records).
        /// </summary>
        /// <param name="industry">The industry to request.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)]
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithDynamicPaths_AndNoQueryString_ShouldRespondWith_200OK_First25RecordsAsync(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            // Arrange
            string conformanceId = Guid.NewGuid().ToString();
            string tokenEndpoint = $"{GenerateDynamicCtsUrl(IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL, conformanceId)}/idp/connect/token";
            var getDataHolderBrandsUrl = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL, conformanceId)}/cdr-register/v1/{industry}/data-holders/brands";
            string expectedDataHolderBrandsUrl = ReplaceSecureHostName(getDataHolderBrandsUrl, DISCOVERY_DOWNSTREAM_BASE_URL);

            var expectedResponse = GetExpectedResponse(expectedDataHolderBrandsUrl, expectedDataHolderBrandsUrl, null, null, null, industry);

            // Arrange - Get access token
            var accessToken = await new IntegrationTests.Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = ReplaceSecureHostName(tokenEndpoint, IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL),
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME,
            }.GetAsync(addCertificateToRequest: false);

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = getDataHolderBrandsUrl,
                AccessToken = accessToken,
                XV = EndpointVersion,
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check XV
                    Assert_HasHeader(api.XV, response.Headers, "x-v");

                    // Assert - Check json
                    await Assert_HasContent_Json(expectedResponse, response.Content);
                }
            }
        }

        /// <summary>
        /// The request with <c>page-size</c> and <c>page</c> query parameter should return with the expected page and record count.
        /// </summary>
        /// <param name="industry">The industry to request.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null, null, 5, HttpStatusCode.NotFound)]
        [InlineData("banking", null, 5)]
        [InlineData("banking", 3, 5)]
        [InlineData("banking", 6, 5)]
        public async Task Get_WithPageSize_ShouldRespondWith_200OK_Page1Of5Records(string? industry, int? page = null, int? pageSize = 5, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            await Test_QueryParameters_RespondWithExpectedStatusAndBody(null, page, pageSize, industry, expectedStatusCode);
        }

        /// <summary>
        /// The request with an <c>updated-since</c> query parameter should return the expected page and record count.
        /// </summary>
        /// <param name="industry">The industry to request.</param>
        /// <param name="updatedSince">The value for <c>updated-since</c> query parameter.</param>
        /// <param name="page">The value for <c>page</c> query parameter.</param>
        /// <param name="pageSize">The value for <c>page-size</c> query parameter.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null, "2021-04-01T00:00:00Z", null, null, HttpStatusCode.NotFound)]
        [InlineData("banking", "2021-04-01T00:00:00Z", 6, 5)]
        [InlineData("banking", "2021-04-30T00:00:00Z", null, null)]
        [InlineData("energy", "2021-04-30T00:00:00Z", null, null)]
        [InlineData("non-bank-lending", "2021-04-30T00:00:00Z", null, null)]
        [InlineData("telco", "2021-04-30T00:00:00Z", null, null)]
        public async Task Get_WithUpdatedSince_ShouldRespondWith_200OK_ExpectedRecordCount(string? industry, string updatedSince, int? page = null, int? pageSize = 5, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var updatedSinceDate = DateTime.Parse(updatedSince, CultureInfo.InvariantCulture);
            await Test_QueryParameters_RespondWithExpectedStatusAndBody(updatedSinceDate, page, pageSize, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.NotFound, null)] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively no date so the filter will not take effect. It won't be invalid, it will be a 200 OK.
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")] // abc in AC
        [InlineData("32/32/2021", HttpStatusCode.BadRequest, "telco")]
        public async Task Get_WithUpdatedSinceInvalidDate_ShouldRespondWith_400BadRequest_InvalidDateTimeString(string updatedSince, HttpStatusCode expectedStatusCode, string? industry)
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands?updated-since={updatedSince}",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    var expectedContent = """
                        {
                            "errors": [
                                {
                                    "code": "urn:au-cds:error:cds-all:Field/InvalidDateTime",
                                    "title": "Invalid DateTime",
                                    "detail": "updated-since should be valid DateTimeString",
                                }
                            ]
                        }
                        """;
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithMissingAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            await Test_WithAccessToken_RespondsWithClientErrorStatusCode(null, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithInvalidAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            await Test_WithAccessToken_RespondsWithClientErrorStatusCode("foo", industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)] // DF: this will be a 404 now.
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithExpiredAccessToken_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            // Arrange
            // Expired at "Tuesday, May 18, 2021 11:33:45 PM GMT+10:00"
            var accessToken = "eyJhbGciOiJQUzI1NiIsImtpZCI6IkFBMjRGMTg1RUUzRjY3NTA0ODA4RkM0RTI2QjEzNUI5OUU2M0JEQTkiLCJ0eXAiOiJhdCtqd3QiLCJ4NXQiOiJxaVR4aGU0X1oxQklDUHhPSnJFMXVaNWp2YWsifQ.eyJuYmYiOjE2MjEzNDQ1MjUsImV4cCI6MTYyMTM0NDgyNSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzAwMC9pZHAiLCJhdWQiOiJjZHItcmVnaXN0ZXIiLCJjbGllbnRfaWQiOiI2ZjdhMWI4ZS04Nzk5LTQ4YTgtOTAxMS1lMzkyMDM5MWY3MTMiLCJqdGkiOiJDODRBNTM5MTA2QjI4NUJBODI2RjZGMDQ3MjU4RjBBNCIsImlhdCI6MTYyMTM0NDUyNSwic2NvcGUiOlsiY2RyLXJlZ2lzdGVyOmJhbms6cmVhZCJdLCJjbmYiOnsieDV0I1MyNTYiOiI1OEQ3NkY3QTYxQ0Q3MjZEQTFDNTRGNjg5OEU4RTY5RUE0Qzg4MDYwIn19.RTU-zrqkb-WXcJzCz62SJ4h19lj8MDyGcvLOmg0qx05WFbAsY4mEP3gsoqM1LJfq4ncw7RqSvbkCNQQ-NOnyoBHF8MGe7mzdUh3YrD0_lTg20Dkx1-l044svtP_CKTI3rXT3bZaYWce0Tb1s3mrJzfN3ja23o93FGR-wbIwHp2347b0DxjznpKBw5meLhAjS7OCx6_uMm1la6IziSQgqMd2WaA-od7w8J5br-Nn-QZZi7X1KGiPEKFDFNk8KrUdPc4NCH6t7f-Sbc34KNNEWfAOJkWdDrmsBaifSlWvSlS4nUnurGHYkmimA2JUuv3ZTqzCcLRamEER1ZoTcIs_PDw";

            await Test_WithAccessToken_RespondsWithClientErrorStatusCode(accessToken, industry, expectedStatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.NotFound)]
        [InlineData("banking")]
        [InlineData("energy")]
        [InlineData("non-bank-lending")]
        [InlineData("telco")]
        public async Task Get_WithDifferentHolderOfKey_ShouldRespondWith_401Unauthorized(string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = ADDITIONAL_CERTIFICATE_FILENAME,  // ie different holder of key
                CertificatePassword = ADDITIONAL_CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.NotFound, null)]
        [InlineData("-1", HttpStatusCode.NotFound, null)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.NotFound, null)]
        [InlineData("foo", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "banking")]
        [InlineData("-1", HttpStatusCode.BadRequest, "banking")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "banking")]
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "energy")]
        [InlineData("-1", HttpStatusCode.BadRequest, "energy")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "energy")]
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "non-bank-lending")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("-1", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("foo", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "telco")]
        [InlineData("-1", HttpStatusCode.BadRequest, "telco")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "telco")]
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")]
        public async Task Get_WithInvalidPageSize_ShouldRespondWith_400BadRequest_PageSizeMustBePositiveInteger(string pageSize, HttpStatusCode expectedStatusCode, string? industry)
        {
            var expectedContent = """
                {
                    "errors": [
                        {
                            "code": "urn:au-cds:error:cds-all:Field/InvalidPageSize",
                            "title": "Invalid Page Size",
                            "detail": "Page size not a positive Integer",
                        }
                    ]
                }
                """;
            await Test_EndpointRespondsSuccessfullyOrWithExpectedError(
                $"page-size={pageSize}",
                expectedStatusCode,
                expectedContent,
                industry);
        }

        [Theory]
        [InlineData("", HttpStatusCode.NotFound, null)] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.NotFound, null)]
        [InlineData("-1", HttpStatusCode.NotFound, null)]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.NotFound, null)]
        [InlineData("foo", HttpStatusCode.NotFound, null)]
        [InlineData("", HttpStatusCode.OK, "banking")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "banking")]
        [InlineData("-1", HttpStatusCode.BadRequest, "banking")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "banking")]
        [InlineData("foo", HttpStatusCode.BadRequest, "banking")]
        [InlineData("", HttpStatusCode.OK, "energy")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "energy")]
        [InlineData("-1", HttpStatusCode.BadRequest, "energy")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "energy")]
        [InlineData("foo", HttpStatusCode.BadRequest, "energy")]
        [InlineData("", HttpStatusCode.OK, "non-bank-lending")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("-1", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("foo", HttpStatusCode.BadRequest, "non-bank-lending")]
        [InlineData("", HttpStatusCode.OK, "telco")] // "" is effectively not providing a page-size, so it will default to 25.
        [InlineData("0", HttpStatusCode.BadRequest, "telco")]
        [InlineData("-1", HttpStatusCode.BadRequest, "telco")]
        [InlineData("99999999999999999999999999999999999999999999999999", HttpStatusCode.BadRequest, "telco")]
        [InlineData("foo", HttpStatusCode.BadRequest, "telco")]
        public async Task Get_WithInvalidPage_ShouldRespondWith_400BadRequest_PageMustBePositiveInteger(string page, HttpStatusCode expectedStatusCode, string? industry)
        {
            var expectedContent = """
                {
                    "errors": [
                        {
                            "code": "urn:au-cds:error:cds-all:Field/Invalid",
                            "title": "Invalid Field",
                            "detail": "Page not a positive integer",
                        }
                    ]
                }
                """;
            await Test_EndpointRespondsSuccessfullyOrWithExpectedError(
                $"page={page}",
                expectedStatusCode,
                expectedContent,
                industry);
        }

        [Theory]
        [InlineData(EndpointVersion, null, HttpStatusCode.NotFound)]
        [InlineData(EndpointVersion, "banking")]
        [InlineData(EndpointVersion, "energy")]
        [InlineData(EndpointVersion, "non-bank-lending")]
        [InlineData(EndpointVersion, "telco")]
        public async Task Get_WithPageOutOfRange_ShouldRespondWith_400BadRequest_PageExceedsMaxNumberOfPages(string page, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            var expectedContent = """
                {
                    "errors": [
                        {
                            "code": "urn:au-cds:error:cds-all:Field/Invalid",
                            "title": "Invalid Field",
                            "detail": "Page is out of range",
                        }
                    ]
                }
                """;
            await Test_EndpointRespondsSuccessfullyOrWithExpectedError(
                $"page={page}",
                expectedStatusCode,
                expectedContent,
                industry);
        }

        [Theory]
        [InlineData("1001", null, HttpStatusCode.NotFound)]
        [InlineData("1001", "banking")]
        [InlineData("1001", "energy")]
        [InlineData("1001", "non-bank-lending")]
        [InlineData("1001", "telco")]
        public async Task Get_WithPageSizeTooLarge_ShouldRespondWith_400BadRequest_PageSizeTooLarge(string pageSize, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest)
        {
            var expectedContent = """
                {
                    "errors": [
                        {
                            "code": "urn:au-cds:error:cds-all:Field/Invalid",
                            "title": "Invalid Field",
                            "detail": "Page size too large",
                        }
                    ]
                }
                """;
            await Test_EndpointRespondsSuccessfullyOrWithExpectedError(
                $"page-size={pageSize}",
                expectedStatusCode,
                expectedContent,
                industry);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NotFound, null)] // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Removed
        [InlineData(3, HttpStatusCode.NotFound, null)] // Suspended
        [InlineData(4, HttpStatusCode.NotFound, null)] // Revoked
        [InlineData(5, HttpStatusCode.NotFound, null)] // Surrendered
        [InlineData(6, HttpStatusCode.NotFound, null)] // Inactive
        [InlineData(1, HttpStatusCode.OK, "banking")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Removed
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Suspended
        [InlineData(4, HttpStatusCode.Forbidden, "banking")] // Revoked
        [InlineData(5, HttpStatusCode.Forbidden, "banking")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "banking")] // Inactive
        [InlineData(1, HttpStatusCode.OK, "energy")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Removed
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Suspended
        [InlineData(4, HttpStatusCode.Forbidden, "energy")] // Revoked
        [InlineData(5, HttpStatusCode.Forbidden, "energy")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "energy")] // Inactive
        [InlineData(1, HttpStatusCode.OK, "non-bank-lending")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "non-bank-lending")] // Removed
        [InlineData(3, HttpStatusCode.Forbidden, "non-bank-lending")] // Suspended
        [InlineData(4, HttpStatusCode.Forbidden, "non-bank-lending")] // Revoked
        [InlineData(5, HttpStatusCode.Forbidden, "non-bank-lending")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "non-bank-lending")] // Inactive
        [InlineData(1, HttpStatusCode.OK, "telco")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Removed
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Suspended
        [InlineData(4, HttpStatusCode.Forbidden, "telco")] // Revoked
        [InlineData(5, HttpStatusCode.Forbidden, "telco")] // Surrendered
        [InlineData(6, HttpStatusCode.Forbidden, "telco")] // Inactive
        public async Task Get_WithDataRecipientNotActive_ShouldRespondWith_403Forbidden(
            int participationStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            var saveParticipationStatusId = GetParticipationStatusId(ParticipationId);

            await Test_EndpointValidatesAdrStatus(
                expectedStatusCode,
                beforeRequest: () => SetParticipationStatusId(ParticipationId, participationStatusId),
                afterRequest: () => SetParticipationStatusId(ParticipationId, saveParticipationStatusId),
                industry: industry);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NotFound, null)] // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Inactive
        [InlineData(3, HttpStatusCode.NotFound, null)] // Removed
        [InlineData(1, HttpStatusCode.OK, "banking")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Removed
        [InlineData(1, HttpStatusCode.OK, "energy")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Removed
        [InlineData(1, HttpStatusCode.OK, "non-bank-lending")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "non-bank-lending")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "non-bank-lending")] // Removed
        [InlineData(1, HttpStatusCode.OK, "telco")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Removed
        public async Task Get_WithDataRecipientBrandNotActive_ShouldRespondWith_403Forbidden(
            int brandStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            int saveBrandStatusId = GetBrandStatusId(BrandId);

            await Test_EndpointValidatesAdrStatus(
                expectedStatusCode,
                beforeRequest: () => SetBrandStatusId(BrandId, brandStatusId),
                afterRequest: () => SetBrandStatusId(BrandId, saveBrandStatusId),
                industry: industry);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.NotFound, null)] // Active
        [InlineData(2, HttpStatusCode.NotFound, null)] // Inactive
        [InlineData(3, HttpStatusCode.NotFound, null)] // Removed
        [InlineData(1, HttpStatusCode.OK, "banking")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "banking")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "banking")] // Removed
        [InlineData(1, HttpStatusCode.OK, "energy")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "energy")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "energy")] // Removed
        [InlineData(1, HttpStatusCode.OK, "non-bank-lending")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "non-bank-lending")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "non-bank-lending")] // Removed
        [InlineData(1, HttpStatusCode.OK, "telco")] // Active
        [InlineData(2, HttpStatusCode.Forbidden, "telco")] // Inactive
        [InlineData(3, HttpStatusCode.Forbidden, "telco")] // Removed
        public async Task Get_WithDataRecipientSoftwareProductNotActive_ShouldRespondWith_403Forbidden(
            int softwareProductStatusId,
            HttpStatusCode expectedStatusCode,
            string? industry)
        {
            int saveSoftwareProductStatusId = GetSoftwareProductStatusId(SoftwareProductId);

            await Test_EndpointValidatesAdrStatus(
                expectedStatusCode,
                beforeRequest: () => SetSoftwareProductStatusId(SoftwareProductId, softwareProductStatusId),
                afterRequest: () => SetSoftwareProductStatusId(SoftwareProductId, saveSoftwareProductStatusId),
                industry: industry);
        }

        [Theory]
        [InlineData("all")]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag(string? industry)
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            // Arrange - Get brands and save the ETag
            var expectedETag = (await new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
                IfNoneMatch = null, // ie If-None-Match is not set
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act - Use Etag
            var response = await new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
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
        [InlineData("foo")]
        public async Task Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest(string industry)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    var expectedContent = @"
                            {
                                ""errors"": [
                                    {
                                    ""code"": ""urn:au-cds:error:cds-all:Field/Invalid"",
                                    ""title"": ""Invalid Field"",                                        
                                    ""detail"": ""industry"",
                                    }
                                ]
                            }";
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        [Theory]
        [InlineData(null, "cdr-register:read", HttpStatusCode.NotFound)] // No industry
        [InlineData("banking", "cdr-register:read", HttpStatusCode.OK)]
        [InlineData("energy", "cdr-register:read", HttpStatusCode.OK)]
        [InlineData("non-bank-lending", "cdr-register:read", HttpStatusCode.OK)]
        [InlineData("telco", "cdr-register:read", HttpStatusCode.OK)]
        public async Task Get_WithScope_ShouldRespondWith_200OK(string? industry, string scope, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = scope,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Theory]
        [InlineData(EndpointVersion, UnsupportedEndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Should return current version - x-min-v is ignored when > x-v
        [InlineData(EndpointVersion, LegacyEndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Should return current version - x-v is supported and higher than x-min-v
        [InlineData(EndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Should return current version - x-v is supported equal to x-min-v
        [InlineData(UnsupportedEndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "")] // Valid. Should return current version - x-v is NOT supported and x-min-v is supported
        [InlineData(EndpointVersion, UnsupportedEndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "BANKing")] // Valid. Should return current version - x-min-v is ignored when > x-v
        [InlineData(EndpointVersion, LegacyEndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "Telco")] // Valid. Should return current version - x-v is supported and higher than x-min-v
        [InlineData(EndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "energy")] // Valid. Should return current version - x-v is supported equal to x-min-v
        [InlineData(EndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "mining")] // Invalid Industry

        [InlineData(EndpointVersion, "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is supported but x-min-v (not a positive integer)
        [InlineData("99", "foo", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is not supported and x-min-v (not a positive integer)
        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Unsupported. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, UnsupportedEndpointVersion, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. Both x-v and x-min-v exceed supported version of 2
        [InlineData("1", null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is an obsolete version
        [InlineData("foo", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("0", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (not a positive integer) is invalid with missing x-min-v
        [InlineData("foo", EndpointVersion, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v is invalid with valid x-min-v
        [InlineData("-1", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR)] // Invalid. x-v (negative integer) is invalid with missing x-min-v
        [InlineData(UnsupportedEndpointVersion, null, "N/A", HttpStatusCode.NotAcceptable, false, EXPECTED_UNSUPPORTED_ERROR)] // Unsupported. x-v is higher than supported version of 2
        [InlineData("", null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is an empty string
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR)] // Invalid. x-v header is missing

        // Also check industry specific calls
        [InlineData(UnsupportedEndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "", "banking")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported
        [InlineData(UnsupportedEndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "", "energy")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported
        [InlineData(UnsupportedEndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "", "non-bank-lending")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported
        [InlineData(UnsupportedEndpointVersion, EndpointVersion, EndpointVersion, HttpStatusCode.OK, true, "", "telco")] // Valid. Should return v2 - x-v is NOT supported and x-min-v is supported

        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR, "banking")] // Unsupported. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR, "energy")] // Unsupported. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR, "non-bank-lending")] // Unsupported. x-v is not supported and x-min-v invalid
        [InlineData(UnsupportedEndpointVersion, "0", "N/A", HttpStatusCode.BadRequest, false, EXPECTED_INVALID_VERSION_ERROR, "telco")] // Unsupported. x-v is not supported and x-min-v invalid
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR, "banking")] // Invalid. x-v header is missing
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR, "energy")] // Invalid. x-v header is missing
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR, "non-bank-lending")] // Invalid. x-v header is missing
        [InlineData(null, null, "N/A", HttpStatusCode.BadRequest, false, EXPECTED_MISSING_X_V_ERROR, "telco")] // Invalid. x-v header is missing

        public async Task ValidateVersionHeader(string? xv, string? minXv, string expectedXv, HttpStatusCode expectedHttpStatusCode, bool isExpectedToBeSupported, string expectedError, string industry = "all")
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = xv,
                XMinV = minXv,
            };

            // Act
            var response = await api.SendAsync();

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
                    await Assert_HasContent_Json(expectedError, response.Content);
                }
            }
        }

        private static async Task Test_QueryParameters_RespondWithExpectedStatusAndBody(
            DateTime? updatedSince,
            int? queryPage,
            int? queryPageSize,
            string? industry = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            static string GetUrl(string baseUrl, DateTime? updatedSince, int? queryPage, int? queryPageSize)
            {
                // Build query
                var query = new KeyValuePairBuilder();

                if (updatedSince != null)
                {
                    query.Add("updated-since", ((DateTime)updatedSince).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                }

                if (queryPage != null)
                {
                    query.Add("page", queryPage.Value);
                }

                if (queryPageSize != null)
                {
                    query.Add("page-size", queryPageSize.Value);
                }

                return query.Count > 0 ?
                    $"{baseUrl}?{query.Value}" :
                    baseUrl;
            }

            // Arrange
            var baseUrl = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands";
            var url = GetUrl(baseUrl, updatedSince, queryPage, queryPageSize);

            var expectedResponse = GetExpectedResponse(baseUrl, url, updatedSince, queryPage, queryPageSize, industry);

            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = url,
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check XV
                    Assert_HasHeader(api.XV, response.Headers, "x-v");

                    // Assert - Check json
                    await Assert_HasContent_Json(expectedResponse, response.Content);
                }
            }
        }

        private static string GetExpectedResponse(string baseUrl, string selfUrl, DateTime? updatedSince, int? requestedPage, int? requestedPageSize, string? industry = null)
        {
            static string Link(string baseUrl, DateTime? updatedSince, int? page = null, int? pageSize = null)
            {
                var query = new KeyValuePairBuilder();

                if (updatedSince != null)
                {
                    query.Add("updated-since", ((DateTime)updatedSince).ToString("yyyy-MM-ddTHH\\%3Amm\\%3Ass.fffffffZ"));
                }

                if (page != null)
                {
                    query.Add("page", page.Value);
                }

                if (pageSize != null)
                {
                    query.Add("page-size", pageSize.Value);
                }

                return query.Count == 0 ?
                    baseUrl :
                    $"{baseUrl}?{query.Value}";
            }

            if (industry == "all")
            {
                industry = null; // treat "all" same as no industry
            }

            var page = requestedPage ?? 1;
            var pageSize = requestedPageSize ?? 25;

            using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

            var allData = dbContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Where(brand =>
                    brand.Participation.ParticipationTypeId == ParticipationTypes.Dh &&
                    (industry == null || (industry != null && brand.Participation.Industry.IndustryTypeCode == industry)))
                .Where(brand => brand.Participation.StatusId == ParticipationStatusType.Active)
                .Where(brand => brand.BrandStatusId == BrandStatusType.Active)
                .Where(brand => updatedSince == null || brand.LastUpdated > updatedSince);

            var totalRecords = allData.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            const int minPage = 1;
            if (page < minPage)
            {
                throw new Exception($"Page {page} out of range. Min Page is {minPage}");
            }

            var maxPage = ((totalRecords - 1) / pageSize) + 1;
            if (page > maxPage)
            {
                throw new Exception($"Page {page} out of range. Max Page is {maxPage} (Records={totalRecords}, PageSize={pageSize})");
            }

            var data = allData
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(brand => new
                {
                    dataHolderBrandId = brand.BrandId,
                    brandName = brand.BrandName,
                    brandGroup = brand.BrandGroup,
                    industries = new string[] { brand.Participation.Industry.IndustryTypeCode.ToLower() },
                    logoUri = brand.LogoUri,
                    legalEntity = new
                    {
                        legalEntityId = brand.Participation.LegalEntity.LegalEntityId,
                        legalEntityName = brand.Participation.LegalEntity.LegalEntityName,
                        logoUri = brand.Participation.LegalEntity.LogoUri,
                        registrationNumber = brand.Participation.LegalEntity.RegistrationNumber,
                        registrationDate = brand.Participation.LegalEntity.RegistrationDate == null ? null : brand.Participation.LegalEntity.RegistrationDate.Value.ToString("yyyy-MM-dd"),
                        registeredCountry = brand.Participation.LegalEntity.RegisteredCountry,
                        abn = brand.Participation.LegalEntity.Abn,
                        acn = brand.Participation.LegalEntity.Acn,
                        arbn = brand.Participation.LegalEntity.Arbn,
                        anzsicDivision = brand.Participation.LegalEntity.AnzsicDivision,
                        organisationType = brand.Participation.LegalEntity.OrganisationType.OrganisationTypeCode,
                        status = brand.Participation.Status.ParticipationStatusCode.ToUpper(),
                    },
                    status = brand.BrandStatus.BrandStatusCode,
                    endpointDetail = new
                    {
                        version = brand.Endpoint.Version,
                        publicBaseUri = brand.Endpoint.PublicBaseUri,
                        productBaseUri = brand.Endpoint.ProductBaseUri,
                        resourceBaseUri = brand.Endpoint.ResourceBaseUri,
                        infosecBaseUri = brand.Endpoint.InfosecBaseUri,
                        extensionBaseUri = brand.Endpoint.ExtensionBaseUri,
                        websiteUri = brand.Endpoint.WebsiteUri,
                    },
                    authDetails = brand.AuthDetails.Select(authDetails => new
                    {
                        registerUType = authDetails.RegisterUType.RegisterUTypeCode,
                        jwksEndpoint = authDetails.JwksEndpoint,
                    }),
                    lastUpdated = brand.LastUpdated.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                })
                .ToList();

            var expectedResponse = new
            {
                data,
                links = new
                {
                    first = totalPages == 0 ?
                        null :
                        Link(baseUrl, updatedSince, 1, pageSize),
                    last = totalPages == 0 ?
                        null :
                        Link(baseUrl, updatedSince, totalPages, pageSize),
                    next = totalPages == 0 || page == totalPages ?
                        null :
                        Link(baseUrl, updatedSince, page + 1, pageSize),
                    prev = totalPages == 0 || page == 1 ?
                        null :
                        Link(baseUrl, updatedSince, page - 1, pageSize),
                    self = selfUrl,
                },
                meta = new
                {
                    totalRecords,
                    totalPages,
                },
            };

            return JsonConvert.SerializeObject(expectedResponse);
        }

        /// <summary>
        /// Ensures that the endpoint responds with the expected client error status code for a given access token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="industry">The industry.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task Test_WithAccessToken_RespondsWithClientErrorStatusCode(string? accessToken, string? industry, HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized)
        {
            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        /// <summary>
        /// Validate that the request with the given query string and valid authorization responds successfully, or with an appropriate error message.
        /// </summary>
        /// <param name="queryString">The query string to test.</param>
        /// <param name="expectedStatusCode">The expected response status.</param>
        /// <param name="expectedContent">The expected response error content.</param>
        /// <param name="industry">The industry.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task Test_EndpointRespondsSuccessfullyOrWithExpectedError(string queryString, HttpStatusCode expectedStatusCode, string expectedContent, string? industry)
        {
            // Arrange
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands?{queryString}",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            // Act
            var response = await api.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(expectedStatusCode);

                // Assert - Check error response
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                {
                    // Assert - Check content type
                    Assert_HasContentType_ApplicationJson(response.Content);

                    // Assert - Check error response
                    await Assert_HasContent_Json(expectedContent, response.Content);
                }
            }
        }

        /// <summary>
        /// Validates the ADR status is validated.
        /// </summary>
        /// <param name="expectedStatusCode">The expected response status.</param>
        /// <param name="beforeRequest">Set up actions to change state prior to making the request.</param>
        /// <param name="afterRequest">Post-request actions to reset state.</param>
        /// <param name="industry">The industry.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task Test_EndpointValidatesAdrStatus(
           HttpStatusCode expectedStatusCode,
           BeforeRequest? beforeRequest,
           AfterRequest? afterRequest,
           string? industry)
        {
            var accessToken = await new Infrastructure.AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
            }.GetAsync();

            var api = new Infrastructure.Api
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                HttpMethod = HttpMethod.Get,
                URL = $"{MTLS_BaseURL}/cdr-register/v1/{industry}/data-holders/brands",
                AccessToken = accessToken,
                XV = EndpointVersion,
            };

            beforeRequest?.Invoke();
            try
            {
                // Act
                var response = await api.SendAsync();

                // Assert
                using (new AssertionScope())
                {
                    // Assert - Check status code
                    response.StatusCode.Should().Be(expectedStatusCode);

                    // Assert - Check error response
                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
                    {
                        // Assert - Check content type
                        Assert_HasContentType_ApplicationJson(response.Content);

                        // Assert - Check error response
                        var expectedContent = """
                            {
                                "errors": [
                                    {
                                        "code": "urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive",
                                        "title": "ADR Status Is Not Active",
                                        "detail": "",
                                    }
                                ]
                            }
                            """;
                        await Assert_HasContent_Json(expectedContent, response.Content);
                    }
                }
            }
            finally
            {
                afterRequest?.Invoke();
            }
        }
    }
}
