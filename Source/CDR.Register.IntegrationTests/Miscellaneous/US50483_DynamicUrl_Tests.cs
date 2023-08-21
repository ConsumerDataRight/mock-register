using FluentAssertions.Execution;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using FluentAssertions;
using CDR.Register.IntegrationTests.Infrastructure;
using static CDR.Register.IntegrationTests.Models.DataRecipientMetadata;
using static System.Formats.Asn1.AsnWriter;
using System.Security.Cryptography.X509Certificates;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.Threading;
using CDR.Register.IntegrationTests.API.Update;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CDR.Register.IntegrationTests.Extensions;

namespace CDR.Register.IntegrationTests.Miscellaneous
{
    public class US50483_DynamicUrl_Tests : BaseTest
    {
        public US50483_DynamicUrl_Tests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        private const string DEFAULT_SOFTWAREPRODUCT_ID = "86ECB655-9EBA-409C-9BE3-59E7ADF7080D";

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData("Invalid Certificate Thumbprint", "foo", DEFAULT_CERTIFICATE_COMMON_NAME)]
        public async Task AC01_Get_Data_Holder_Brands_Invalid_Certificate_Header_For_Access_Token(string testDescription, string certificateThumbPrint, string certificateCommonName)
        {
            Log.Information($"Executing test for {testDescription}.\nCertificate ThumbPrint: {certificateThumbPrint}.\nCertificateCommonName: {certificateCommonName}");

            // Arrange
            string conformanceId = Guid.NewGuid().ToString();
            string tokenEndpoint = $"{GenerateDynamicCtsUrl(IDENTITY_PRIVIDER_DOWNSTREAM_BASE_URL, conformanceId)}/idp/connect/token";
            var getDataholderBrandsUrl = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL, conformanceId)}/cdr-register/v1/banking/data-holders/brands";

            // Arrange - Get access token
            var accessToken = await new AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = tokenEndpoint,
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME
            }.GetAsync(addCertificateToRequest: false);

            var api = new Infrastructure.API
            {

                HttpMethod = HttpMethod.Get,
                URL = getDataholderBrandsUrl,
                AccessToken = accessToken,
                XV = "2",
                CertificateThumbprint = certificateThumbPrint,
                CertificateCn = certificateCommonName
            };

            // Act
            var response = await api.SendAsync();

            Log.Information($"Response from {getDataholderBrandsUrl} Endpoint: {response.StatusCode} \n{response.Content.ReadAsStringAsync().Result}");

            // Assert
            await VerifyInvalidTokenRepsonse(response);

        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData("Invalid Certificate Common Name", DEFAULT_CERTIFICATE_THUMBPRINT, "foo")]
        [InlineData("Missing Certificate Thumbprint", "", DEFAULT_CERTIFICATE_COMMON_NAME)]
        public async Task AC02_Get_Access_Token_Invalid_Certificate_Header_For_Access_Token(string testDescription, string certificateThumbPrint, string certificateCommonName)
        {

            Log.Information($"Executing test for {testDescription}.\nCertificate ThumbPrint: {certificateThumbPrint}.\nCertificateCommonName: {certificateCommonName}");

            // Arrange
            string conformanceId = Guid.NewGuid().ToString();
            string tokenEndpoint = $"{GenerateDynamicCtsUrl(IDENTITY_PRIVIDER_DOWNSTREAM_BASE_URL, conformanceId)}/idp/connect/token";

            // Arrange - Get access token
            var accessToken = new AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = tokenEndpoint,
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = certificateThumbPrint,
                CertificateCn = certificateCommonName
            };

            HttpResponseMessage response = await accessToken.SendAccessTokenRequest(addCertificateToRequest: false);

            // Assert
            using (new AssertionScope())
            {

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                await Assert_HasContent_Json(@"{""error"":""invalid_client"",""error_description"":""Client certificate validation failed""}", response.Content);

            }
        }

        [Trait("Category", "CTSONLY")]
        [Fact]
        public async Task AC03_Get_Access_Token_Invalid_Url_Regular_Expression_For_Access_Token()
        {
            
            string tokenEndpoint = $"{IDENTITY_PRIVIDER_DOWNSTREAM_BASE_URL}/foo/idp/connect/token";
            
            // Arrange - Get access token
            var accessToken = new AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = tokenEndpoint,
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME
            };

            HttpResponseMessage response = await accessToken.SendAccessTokenRequest(addCertificateToRequest: false);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }

        [Trait("Category", "CTSONLY")]
        [Fact] 
        public async Task AC04_Get_Data_Holder_Brands_Invalid_Access_Token_Issuer()
        { 
            // Arrange
            string conformanceId = Guid.NewGuid().ToString();
            string mismatachedConformanceId = Guid.NewGuid().ToString();
            string tokenEndpoint = $"{GenerateDynamicCtsUrl(IDENTITY_PRIVIDER_DOWNSTREAM_BASE_URL, conformanceId)}/idp/connect/token";
            var getDataholderBrandsUrl = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL, mismatachedConformanceId)}/cdr-register/v1/banking/data-holders/brands";

            // Arrange - Get access token
            var accessToken = await new AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = tokenEndpoint,
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME
            }.GetAsync(addCertificateToRequest: false);

            var api = new Infrastructure.API
            {

                HttpMethod = HttpMethod.Get,
                URL = getDataholderBrandsUrl,
                AccessToken = accessToken,
                XV = "2",
                CertificateThumbprint = DEFAULT_CERTIFICATE_THUMBPRINT,
                CertificateCn = DEFAULT_CERTIFICATE_COMMON_NAME
            };

            // Act
            var response = await api.SendAsync();

            Log.Information($"Response from {getDataholderBrandsUrl} Endpoint: {response.StatusCode} \n{response.Content.ReadAsStringAsync().Result}");

            // Assert
            await VerifyInvalidTokenRepsonse(response);
        
        }

        private async Task VerifyInvalidTokenRepsonse(HttpResponseMessage response)
        {
            using (new AssertionScope())
            {

                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                ExpectedErrors expectedErrors = new ExpectedErrors();
                expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.Unauthorized, "invalid_token");

                string actualErrors = await response.Content.ReadAsStringAsync();

                Assert_Json(GetJsonFromModel(expectedErrors), actualErrors);

            }
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData("Both Header and Database using only Common Name",                          DEFAULT_CERTIFICATE_THUMBPRINT, "Test Common Name",                                         "Test Common Name")]
        [InlineData("Both Header and Database using Full Distinguished Name",                   DEFAULT_CERTIFICATE_THUMBPRINT, "CN=TestCommonName1,O=Test Org,OU=Test Org Unit,C=AU",      "CN=TestCommonName1,O=Test Org,OU=Test Org Unit,C=AU")]
        [InlineData("Header using only Common Name and Database using Full Distinguished Name", DEFAULT_CERTIFICATE_THUMBPRINT, "Test Common Name 2",                                       "CN=Test Common Name 2,O=Test Org,OU=Test Org Unit,C=AU")]
        [InlineData("Header using Full Distinguished Name and Database using only Common Name", DEFAULT_CERTIFICATE_THUMBPRINT, "CN=Test Common Name 3,O=Test Org,OU=Test Org Unit,C=AU",    "Test Common Name 3")]

        public async Task AC05_Get_Access_Token_With_Valid_Certificate_Header(string testDescription, string certificateThumbPrint, string certificateCommonName, string dbCommonName)
        {

            Log.Information($"Executing test for {testDescription}.\nCertificate ThumbPrint: {certificateThumbPrint}.\nCertificateCommonName: {certificateCommonName}.\nDatabase Common Name: {dbCommonName}.");

            // Arrange
            string conformanceId = Guid.NewGuid().ToString();
            string tokenEndpoint = $"{GenerateDynamicCtsUrl(IDENTITY_PRIVIDER_DOWNSTREAM_BASE_URL, conformanceId)}/idp/connect/token";
            string getDataholderBrandsUrl = $"{GenerateDynamicCtsUrl(DISCOVERY_DOWNSTREAM_BASE_URL, conformanceId)}/cdr-register/v1/banking/data-holders/brands";

            SetCertificateCommonName(DEFAULT_SOFTWAREPRODUCT_ID, dbCommonName);

            // Arrange - Create access token request
            var accessToken = new AccessToken
            {
                CertificateFilename = CERTIFICATE_FILENAME,
                CertificatePassword = CERTIFICATE_PASSWORD,
                Scope = "cdr-register:read",
                Audience = tokenEndpoint,
                TokenEndPoint = tokenEndpoint,
                CertificateThumbprint = certificateThumbPrint,
                CertificateCn = certificateCommonName
            };

            HttpResponseMessage accessTokenResponse = await accessToken.SendAccessTokenRequest(addCertificateToRequest: false);

            // Assert
            accessTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: testDescription);

            // Send request to get Data Holders to ensure Access token works.
            string validAccessToken = await accessToken.GetAsync();

            var api = new Infrastructure.API
            {

                HttpMethod = HttpMethod.Get,
                URL = getDataholderBrandsUrl,
                AccessToken = validAccessToken,
                XV = "2",
                CertificateThumbprint = certificateThumbPrint,
                CertificateCn = certificateCommonName
            };

            // Act
            HttpResponseMessage getDataHolderResponse = await api.SendAsync();

            Log.Information($"Response from {getDataholderBrandsUrl} Endpoint: {accessTokenResponse.StatusCode} \n{accessTokenResponse.Content.ReadAsStringAsync().Result}");

            getDataHolderResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: $"Get Data Holder should work when{testDescription}");

        }

        private static void SetCertificateCommonName(string softwareProductId, string commonName)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update Common Name
            using var updateCommand = new SqlCommand("UPDATE SoftwareProductCertificate SET CommonName = @commonName WHERE SoftwareProductId = @softwareProductId", connection);
            updateCommand.Parameters.AddWithValue("@softwareProductId", softwareProductId);
            updateCommand.Parameters.AddWithValue("@commonName", commonName);
            updateCommand.ExecuteNonQuery();

            // Check commonName was Updated
            using var selectCommand = new SqlCommand("select count(*) from SoftwareProductCertificate where SoftwareProductId = @softwareProductId and CommonName = @commonName", connection);
            selectCommand.Parameters.AddWithValue("@softwareProductId", softwareProductId);
            selectCommand.Parameters.AddWithValue("@commonName", commonName);
            if (selectCommand.ExecuteScalarInt32() == 0)
            {
                throw new Exception($"Common name was not updated for Software Product: {softwareProductId}");
            };
        }

    }
}
