using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.IntegrationTests.Models;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CDR.Register.IntegrationTests.API.Update
{
    public class US50480_UpdateDataHolders : BaseTest
    {
        public US50480_UpdateDataHolders(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }

        private const string UPDATE_DATA_HOLDER_CURRENT_API_VERSION = "1";

        private const string TEST_DATA_BASE_URI = "https://TestAumationLogoUri.gov.au";

        public enum Industry
        {
            Banking,
            Energy
        }

        [Theory]
        [InlineData(Industry.Banking)]
        [InlineData(Industry.Energy)]
        public async Task AC02_Add_New_DataHolder_With_Only_Mandatory_Fields_Http_200(Industry industry)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(industry, false);

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, response, industry);

            // Assert participation created correctly
            VerifyParticipationRecord(dataHolderMetadata.LegalEntity.LegalEntityId, "DH", industry.ToString(), dataHolderMetadata.Status);

        }

        [Theory]
        [InlineData(Industry.Banking)]
        [InlineData(Industry.Energy)]
        public async Task AC02_Add_New_DataHolder_With_All_Fields_Fields_Http_200(Industry industry)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(industry, true);

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, response, industry);

            VerifyParticipationRecord(dataHolderMetadata.LegalEntity.LegalEntityId, "DH", industry.ToString(), dataHolderMetadata.Status);

        }

        [Fact]
        public async Task AC03_Modify_Existing_DataHolder_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send Original request to Register
            HttpResponseMessage originalResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, originalResponse, Industry.Banking);

            // Modify data holder request
            dataHolderMetadata.BrandName = "Test Automation Modified Brand Name";
            dataHolderMetadata.LegalEntity.LegalEntityName = "Test Automation Modified Legal Entity Name";
            dataHolderMetadata.EndpointDetail.PublicBaseUri = $"{TEST_DATA_BASE_URI}/ModifiedPublicBaseUri";
            dataHolderMetadata.AuthDetails.JwksEndpoint = $"{TEST_DATA_BASE_URI}/ModifiedJwks";

            // Send Modified request to Register
            HttpResponseMessage modifiedResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, modifiedResponse, Industry.Banking);

            VerifyParticipationRecord(dataHolderMetadata.LegalEntity.LegalEntityId, "DH", Industry.Banking.ToString(), dataHolderMetadata.Status);

        }

        [Fact]
        public async Task AC03_Add_New_Brand_To_Existing_Legal_Entity_200()
        {

            // Generate valid payload.
            DataHolderMetadata originalDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);
            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata));

            // Send Original request to Register
            HttpResponseMessage originalResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(originalDataHolderMetadata, originalResponse, Industry.Banking);

            // Generate valid payload.
            DataHolderMetadata newDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            //Set Legal Entity Id to Original Legal Entity Id. Register should attach new brand to Legal Entity
            newDataHolderMetadata.LegalEntity.LegalEntityId = originalDataHolderMetadata.LegalEntity.LegalEntityId;

            //Also modify other fields
            newDataHolderMetadata.BrandName = "Test Automation Modified Brand Name";
            newDataHolderMetadata.EndpointDetail.PublicBaseUri = $"{TEST_DATA_BASE_URI}/ModifiedPublicBaseUri";
            newDataHolderMetadata.AuthDetails.JwksEndpoint = $"{TEST_DATA_BASE_URI}/ModifiedJwks";

            // Send Modified request to Register
            HttpResponseMessage modifiedResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(newDataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(newDataHolderMetadata, modifiedResponse, Industry.Banking);

            //Also Check Original is not affected
            VerifySuccessfulDataHolderUpdate(originalDataHolderMetadata, originalResponse, Industry.Banking);

            VerifyParticipationRecord(newDataHolderMetadata.LegalEntity.LegalEntityId, "DH", Industry.Banking.ToString(), newDataHolderMetadata.Status);

        }

        [Fact]
        public async Task AC03_Add_New_Brand_To_Existing_Legal_Entity_With_Different_Industry_200()
        {

            // Generate valid Banking payload.
            DataHolderMetadata originalDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);
            // Send to Register
            _ = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata));

            // Send Original request to Register
            HttpResponseMessage originalResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(originalDataHolderMetadata, originalResponse, Industry.Banking);

            // Generate valid Energy payload.
            DataHolderMetadata newDataHolderMetadata = CreateValidDataholderMetadata(Industry.Energy, true);

            //Set Legal Entity Id to Original Legal Entity Id. Register should attach new brand to Legal Entity, and create a new participation record.
            newDataHolderMetadata.LegalEntity.LegalEntityId = originalDataHolderMetadata.LegalEntity.LegalEntityId;

            //Also modify other fields
            newDataHolderMetadata.BrandName = "Test Automation Energy Brand Name";
            newDataHolderMetadata.EndpointDetail.PublicBaseUri = $"{TEST_DATA_BASE_URI}/EnergyPublicBaseUri";
            newDataHolderMetadata.AuthDetails.JwksEndpoint = $"{TEST_DATA_BASE_URI}/EnergyJwks";

            // Send Modified request to Register
            HttpResponseMessage modifiedResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(newDataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(newDataHolderMetadata, modifiedResponse, Industry.Energy);

            // Verify Banking Participant
            VerifyParticipationRecord(originalDataHolderMetadata.LegalEntity.LegalEntityId, "DH", Industry.Banking.ToString(), originalDataHolderMetadata.Status);

            // Verify Energy(subsequent) Participant
            VerifyParticipationRecord(newDataHolderMetadata.LegalEntity.LegalEntityId, "DH", Industry.Energy.ToString(), newDataHolderMetadata.Status);

        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AC04_Missing_Version_In_Header_Http_400(string xv)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata originalDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingHeader, "An API version x-v header is required, but was not specified.");

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1.1")]
        public async Task AC05_Invalid_Version_Http_400(string xv)
        {
            DataHolderMetadata originalDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidVersion, "Version is not a positive Integer.");

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);
        }


        [Theory]
        [InlineData("20")]
        public async Task AC10_Unsupported_Version_Http_400(string xv)
        {
            DataHolderMetadata originalDataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataHolderRequest(GetJsonFromModel(originalDataHolderMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.UnsupportedVersion, "Requested version is lower than the minimum version or greater than maximum version.");

            // Assert Response
            await VerifyNotAcceptableRequest(expectedErrors, response);
        }

        [Theory]
        [InlineData("Missing Data Holder Brand Id", "dataHolderBrandId")]
        [InlineData("Missing Data Holder Brand Name", "brandName")]
        [InlineData("Missing Data Holder Industries", "industries")]
        [InlineData("Missing Data Holder LogoUri", "logoUri")]
        [InlineData("Missing Data Holder Status", "status", true)]
        [InlineData("Missing Data Holder Legal Entity", "legalEntity")]
        [InlineData("Missing Data Holder Endpoint Detail", "endpointDetail")]
        [InlineData("Missing Data Holder Auth Details", "authDetails")]
        [InlineData("Missing Data Legal Entity Id", "legalEntity.legalEntityId")]
        [InlineData("Missing Data Legal Entity Name", "legalEntity.legalEntityName")]
        [InlineData("Missing Data Legal Entity Logo Uri", "legalEntity.logoUri")]
        [InlineData("Missing Data Legal Entity Status", "legalEntity.status", true)]
        [InlineData("Missing Data Holder Endpoint Detail Version", "endpointDetail.version")]
        [InlineData("Missing Data Holder Endpoint Detail Public Base Uri", "endpointDetail.publicBaseUri")]
        [InlineData("Missing Data Holder Endpoint Detail Resource Base Uri", "endpointDetail.resourceBaseUri")]
        [InlineData("Missing Data Holder Endpoint Detail Infosec Base Uri", "endpointDetail.infosecBaseUri")]
        [InlineData("Missing Data Holder Endpoint Detail Website Uri", "endpointDetail.websiteUri")]
        [InlineData("Missing Data Holder Auth Details Register U Type", "authDetails.registerUType", true)]
        [InlineData("Missing Data Holder Auth Details Jwks Endpoint", "authDetails.jwksEndpoint")]
        public async Task AC06_Missing_Mandatory_Fields_Http_400(string testId, string elementToRemove, bool expectEnumErrorAsWell = false)
        {
            Log.Information("Executing test for: {TestId}", testId);

            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information("original:\n{JsonModel}", GetJsonFromModel(dataHolderMetadata));

            dataHolderMetadata = RemoveModelElementBasedOnJsonPath(dataHolderMetadata, elementToRemove);
            Log.Information("modified (element removed):\n{JsonModel}", GetJsonFromModel(dataHolderMetadata));

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            string expectedMissingField = ConvertJsonPathToPascalCase(elementToRemove);

            ExpectedErrors expectedErrors = new ExpectedErrors();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, $"{expectedMissingField}");

            if (expectEnumErrorAsWell)
            {
                expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, $"Value '' is not allowed for {expectedMissingField}");
            }

            await VerifyBadRequest(expectedErrors, response);
        }

        [Theory]
        [InlineData("Max Length Data Holder Brand - Name", "brandName", 200)]
        [InlineData("Max Length Data Holder - LogoUri", "logoUri", 1000)]
        [InlineData("Max Length EndPoint - Version", "endpointDetail.version", 25)]
        [InlineData("Max Length EndPoint - PublicBaseUri", "endpointDetail.publicBaseUri", 1000)]
        [InlineData("Max Length EndPoint - ResourceBaseUri", "endpointDetail.resourceBaseUri", 1000)]
        [InlineData("Max Length EndPoint - InfosecBaseUri", "endpointDetail.infosecBaseUri", 1000)]
        [InlineData("Max Length EndPoint - ExtensionBaseUri", "endpointDetail.extensionBaseUri", 1000)]
        [InlineData("Max Length EndPoint - WebsiteUri", "endpointDetail.websiteUri", 1000)]
        [InlineData("Max Length Legal Entity - Name", "legalEntity.legalEntityName", 200)]
        //we don't check accredatation number for DHs as they don't have it.
        [InlineData("Max Length Legal Entity - logoUri", "legalEntity.logoUri", 1000)]
        [InlineData("Max Length Legal Entity - Registration Number", "legalEntity.registrationNumber", 100)]
        [InlineData("Max Length Legal Entity - Registered Country", "legalEntity.registeredCountry", 100)]
        [InlineData("Max Length Legal Entity - ABN", "legalEntity.abn", 11)]
        [InlineData("Max Length Legal Entity - ACN", "legalEntity.acn", 9)]
        [InlineData("Max Length Legal Entity - ARBN", "legalEntity.arbn", 9)]
        [InlineData("Max Length Legal Entity - Anzsic Division", "legalEntity.anzsicDivision", 100)]
        [InlineData("Max Length Auth Details - JwksEndpoint", "authDetails.jwksEndpoint", 1000)]
        public async Task ACXX_Maximum_Field_Length_Exceeded(string testId, string elementUnderTest, int maxLength)
        {
            Log.Information("Executing test for: {TestId} : Max Lenght: {MaxLength}", testId, maxLength);

            // Create dataHolder using maximum field length
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);
            string maxLengthValue = maxLength.GenerateRandomString();
            dataHolderMetadata = ReplaceModelValueBasedOnJsonPath(dataHolderMetadata, elementUnderTest, maxLengthValue);
            Log.Information($"+ve Scenario using maximum field lenght of '{maxLength}':\n{GetJsonFromModel(dataHolderMetadata)}");

            // Send and verify positive scenario
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));
            await VerifyInvalidAndValidFieldResponse(response, dataHolderMetadata, ConvertJsonPathToPascalCase(elementUnderTest), maxLengthValue, true);

            // Create dataHolder using maximum field lenght plus one
            string maxLengthPlusOneValue = (maxLength+1).GenerateRandomString();
            dataHolderMetadata = ReplaceModelValueBasedOnJsonPath(dataHolderMetadata, elementUnderTest, maxLengthPlusOneValue);
            Log.Information($"-ve:\n{GetJsonFromModel(dataHolderMetadata)}");

            // Send and verify negative scenario
            HttpResponseMessage responseNegative = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));
            await VerifyInvalidAndValidFieldResponse(responseNegative, dataHolderMetadata, ConvertJsonPathToPascalCase(elementUnderTest), maxLengthPlusOneValue, false);

        }

        [Theory]
        [InlineData("BANKING")]
        [InlineData("ENERGY")]
        [InlineData("Energy")]
        [InlineData("foo", false)]
        public async Task AC07_Valid_And_Invalid_Industry(string industry, bool isValid = true)
        {
            Industry industryEnum;

            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information($"original:\n{GetJsonFromModel(dataHolderMetadata)}");

            dataHolderMetadata.Industries.Clear();
            dataHolderMetadata.Industries.Add(industry);

            Log.Information($"modified:\n{GetJsonFromModel(dataHolderMetadata)}");

            HttpResponseMessage httpResponseMessage = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            switch (industry.ToUpper())
            {

                case "ENERGY":
                    industryEnum = Industry.Energy;
                    break;

                case "BANKING":
                default:
                    industryEnum = Industry.Banking;
                    break;

            }

            dataHolderMetadata.Industries = dataHolderMetadata.Industries.ConvertAll(x => x.ToUpper());

            await VerifyInvalidAndValidFieldResponse(httpResponseMessage, dataHolderMetadata, "Industries[0]", industry, isValid, industryEnum);

        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("REMOVED")]
        [InlineData("INACTIVE")]
        [InlineData("Inactive")]
        [InlineData("foo", false)]
        public async Task AC07_Valid_And_Invalid_Data_Holder_Status(string status, bool isValid = true)
        {
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information($"original:\n{GetJsonFromModel(dataHolderMetadata)}");

            dataHolderMetadata.Status = status;

            Log.Information($"modified:\n{GetJsonFromModel(dataHolderMetadata)}");

            HttpResponseMessage httpResponseMessage = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            dataHolderMetadata.Status = status.ToUpper();

            await VerifyInvalidAndValidFieldResponse(httpResponseMessage, dataHolderMetadata, "Status", status, isValid);

        }

        [Theory]
        [InlineData("SOLE_TRADER")]
        [InlineData("Sole_Trader")]
        [InlineData("COMPANY")]
        [InlineData("PARTNERSHIP")]
        [InlineData("TRUST")]
        [InlineData("GOVERNMENT_ENTITY")]
        [InlineData("OTHER")]
        [InlineData("foo", false)]
        public async Task AC07_Valid_And_Invalid_Data_Holder_Legal_Entity_Organisation_Type(string orgType, bool isValid = true)
        {
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information($"original:\n{GetJsonFromModel(dataHolderMetadata)}");

            dataHolderMetadata.LegalEntity.OrganisationType = orgType;

            Log.Information($"modified:\n{GetJsonFromModel(dataHolderMetadata)}");

            HttpResponseMessage httpResponseMessage = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            dataHolderMetadata.LegalEntity.OrganisationType = orgType.ToUpper();

            await VerifyInvalidAndValidFieldResponse(httpResponseMessage, dataHolderMetadata, "LegalEntity.OrganisationType", orgType, isValid);

        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("Active")]
        [InlineData("INACTIVE")]
        [InlineData("REMOVED")]
        [InlineData("foo", false)]
        public async Task AC07_Valid_And_Invalid_Data_Holder_Legal_Entity_Status(string status, bool isValid = true)
        {
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information($"original:\n{GetJsonFromModel(dataHolderMetadata)}");

            dataHolderMetadata.LegalEntity.Status = status;

            Log.Information($"modified:\n{GetJsonFromModel(dataHolderMetadata)}");

            HttpResponseMessage httpResponseMessage = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            dataHolderMetadata.LegalEntity.Status = status.ToUpper();

            await VerifyInvalidAndValidFieldResponse(httpResponseMessage, dataHolderMetadata, "LegalEntity.Status", status, isValid);

        }

        [Theory]
        [InlineData("SIGNED-JWT")]
        [InlineData("Signed-Jwt")]
        [InlineData("foo", false)]
        public async Task AC07_Valid_And_Invalid_Data_Holder_Auth_Details_RegisterUType(string registerUType, bool isValid = true)
        {
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);
            Log.Information($"original:\n{GetJsonFromModel(dataHolderMetadata)}");

            dataHolderMetadata.AuthDetails.RegisterUType = registerUType;

            HttpResponseMessage httpResponseMessage = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            dataHolderMetadata.AuthDetails.RegisterUType = registerUType.ToUpper();

            Log.Information($"modified:\n{GetJsonFromModel(dataHolderMetadata)}");

            await VerifyInvalidAndValidFieldResponse(httpResponseMessage, dataHolderMetadata, "AuthDetails.RegisterUType", registerUType, isValid);

        }

        [Fact]
        public async Task AC08_Brand_Already_Associated_With_Different_Legal_Entity_400()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send Original request to Register
            HttpResponseMessage originalResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, originalResponse, Industry.Banking);

            // Set Legal Entity id to a different Legal Entity id.
            // Data Holder request becomes invalid since Brand id is already associated to a different Legal Entity id.
            dataHolderMetadata.LegalEntity.LegalEntityId = Guid.NewGuid().ToString();

            HttpResponseMessage modifiedResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));
            await VerifyInvalidPayloadResponse(modifiedResponse, $"Brand {dataHolderMetadata.DataHolderBrandId} is already associated with a different legal entity.");
        }

        [Fact]
        public async Task AC09_Brand_Already_Associated_With_Same_Legal_Entity_And_Different_Industry_400()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, true);

            // Send Original request to Register
            HttpResponseMessage originalResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, originalResponse, Industry.Banking);

            // Set Industry to a different Industry.
            // Data Holder request becomes invalid since Brand id is already associated to a Legal Entity id with a different industry type.
            dataHolderMetadata.Industries.Clear();
            dataHolderMetadata.Industries.Add(Industry.Energy.ToString());

            HttpResponseMessage modifiedResponse = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata));
            await VerifyInvalidPayloadResponse(modifiedResponse, $"Brand {dataHolderMetadata.DataHolderBrandId} is already associated with the same legal entity in a different industry.");

        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(Industry.Banking)]
        [InlineData(Industry.Energy)]
        public async Task AC02_Add_New_DataHolder_WithValidToken_200(Industry industry)
        {
            // Get the token
            var accessToken = await GetAzureAdAccessToken();

            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(industry, false);

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata), accessToken: accessToken);

            // Assert http response and database updates.
            VerifySuccessfulDataHolderUpdate(dataHolderMetadata, response, industry);

            // Assert participation created correctly
            VerifyParticipationRecord(dataHolderMetadata.LegalEntity.LegalEntityId, "DH", industry.ToString(), dataHolderMetadata.Status);
        }

        [Trait("Category", "CTSONLY")]
        [Theory]
        [InlineData(null)]
        [InlineData(EXPIRED_ACCESS_TOKEN)]
        public async Task AC02_Add_New_DataHolder_InvalidToken_401(string accessToken)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata), accessToken: accessToken);

            // Verify
            await VerifyUnauthorisedRequest(response);
        }

        [Trait("Category", "CTSONLY")]
        [Fact]
        public async Task AC02_Add_New_DataHolder_InvalidRole_401()
        {
            // Get the token
            var accessToken = await GetInvalidAzureAdAccessToken();

            // Generate valid payload with only mandatory/minimun fields.
            DataHolderMetadata dataHolderMetadata = CreateValidDataholderMetadata(Industry.Banking, false);

            // Send to Register
            HttpResponseMessage response = await PostUpdateDataHolderRequest(GetJsonFromModel(dataHolderMetadata), accessToken: accessToken);

            // Verify
            await VerifyUnauthorisedRequest(response);
        }

        private static async Task<HttpResponseMessage> PostUpdateDataHolderRequest(string payload, string xv = UPDATE_DATA_HOLDER_CURRENT_API_VERSION, string accessToken = null)
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var client = new HttpClient(clientHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ADMIN_BaseURL}/admin/metadata/data-holders");

            request.Content = new StringContent($"{payload}", Encoding.UTF8, "application/json");

            if (xv != null)
            {
                request.Headers.Add("x-v", xv);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            }

            Log.Information($"Sending payload:\n{payload}");
            Log.Information($"Request Headers:\n {request.Headers}");

            HttpResponseMessage httpResponse = await client.SendAsync(request);
            Log.Information($"Response from admin/metadata/data-holders API: {httpResponse.StatusCode} \n{httpResponse.Content.ReadAsStringAsync().Result}");

            return httpResponse;

        }

        private static async Task VerifyInvalidAndValidFieldResponse(HttpResponseMessage response, DataHolderMetadata dataHolderMetadata, string field, string value, bool isValid, Industry industry = Industry.Banking)
        {
            // var response = await SendToStub(GetJsonFromModel(dataHolderMetadata), isValid ? "PASS" : "InvalidField");

            if (isValid)
            {
                string actualDataHolder = GetActualDataHolderFromDatabase(dataHolderMetadata.LegalEntity.LegalEntityId, dataHolderMetadata.DataHolderBrandId, industry);

                Assert_Json(GetJsonFromModel(dataHolderMetadata), actualDataHolder);
            }
            else
            {
                ExpectedErrors expectedErrors = new ExpectedErrors();
                expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, $"Value '{value}' is not allowed for {field}");

                await VerifyBadRequest(expectedErrors, response);
            }
        }

        private static async Task VerifyInvalidPayloadResponse(HttpResponseMessage response, string expectedErrorMessage)
        {

            ExpectedErrors expectedErrors = new ExpectedErrors();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, expectedErrorMessage);

            await VerifyBadRequest(expectedErrors, response);
        }

        private static void VerifySuccessfulDataHolderUpdate(DataHolderMetadata expectedDataHolderMetadata, HttpResponseMessage httpResponseFromRegister, Industry industry)
        {
            using (new AssertionScope())
            {
                //Check status code
                httpResponseFromRegister.StatusCode.Should().Be(HttpStatusCode.OK);

                string actualDataHolder = GetActualDataHolderFromDatabase(expectedDataHolderMetadata.LegalEntity.LegalEntityId, expectedDataHolderMetadata.DataHolderBrandId, industry);

                Assert_Json(GetJsonFromModel(expectedDataHolderMetadata), actualDataHolder);

                VerifyBrandLastUpdatedDateRecord(expectedDataHolderMetadata.LegalEntity.LegalEntityId);
            }
        }

        private static DataHolderMetadata CreateValidDataholderMetadata(Industry industry, bool includeOptionalFields = false)
        {

            DataHolderMetadata dataHolderMetadata = new DataHolderMetadata();
            dataHolderMetadata.DataHolderBrandId = Guid.NewGuid().ToString();
            dataHolderMetadata.BrandName = "Test Automation Brand Name";
            dataHolderMetadata.Industries = new List<string> { industry.ToString().ToUpper() };
            dataHolderMetadata.Status = "ACTIVE";
            dataHolderMetadata.LogoUri = $"{TEST_DATA_BASE_URI}/logo.png";

            dataHolderMetadata.LegalEntity = new DataHolderMetadata.LegalEntityChild()
            {
                LegalEntityId = Guid.NewGuid().ToString(),
                LegalEntityName = "Test Automation Generated Legal Entity Name",
                LogoUri = $"{TEST_DATA_BASE_URI}/logo.png",
                Status = "ACTIVE",
            };

            dataHolderMetadata.EndpointDetail = new DataHolderMetadata.EndpointDetailChild()
            {
                Version = "1",
                PublicBaseUri = $"{TEST_DATA_BASE_URI}/publicBaseUri",
                ResourceBaseUri = $"{TEST_DATA_BASE_URI}/resourceBaseUri",
                InfosecBaseUri = $"{TEST_DATA_BASE_URI}/infosecBaseUri",
                WebsiteUri = $"{TEST_DATA_BASE_URI}/websiteUri",
            };

            dataHolderMetadata.AuthDetails = new DataHolderMetadata.AuthDetailsChild()
            {
                RegisterUType = "SIGNED-JWT",
                JwksEndpoint = $"{TEST_DATA_BASE_URI}/jwks.json",
            };

            if (includeOptionalFields)
            {
                dataHolderMetadata.LegalEntity.RegistrationNumber = "AutoRegistrationNo.0123456789";
                dataHolderMetadata.LegalEntity.RegistrationDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                dataHolderMetadata.LegalEntity.RegisteredCountry = "New Zealand";
                dataHolderMetadata.LegalEntity.Abn = "80000000001";
                dataHolderMetadata.LegalEntity.Acn = "123456789";
                dataHolderMetadata.LegalEntity.Arbn = "987654321";
                dataHolderMetadata.LegalEntity.AnzsicDivision = "Test Automation Anzsic Division";
                dataHolderMetadata.LegalEntity.OrganisationType = "SOLE_TRADER";
                dataHolderMetadata.EndpointDetail.ExtensionBaseUri = $"{TEST_DATA_BASE_URI}/extensionBaseUri";
            };

            return dataHolderMetadata;

        }

        private static string GetActualDataHolderFromDatabase(string legalEntityId, string brandId, Industry industry)
        {

            var legalEntityIdGuid = Guid.Parse(legalEntityId);

            try
            {
                using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

                var expectedDataHolder =
                    dbContext.Participations.AsNoTracking()
                        .Include(participation => participation.Status)
                        .Include(participation => participation.Industry)
                        .Include(participation => participation.LegalEntity)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brandStatus => brandStatus.BrandStatus)
                        .Include(participation => participation.Brands)
                        .Where(participation => participation.LegalEntityId == legalEntityIdGuid
                                && participation.Industry.IndustryTypeCode == industry.ToString())
                        .OrderBy(participation => participation.LegalEntityId)
                        .Select(participation => participation.Brands.Where(brand => brand.BrandId == Guid.Parse(brandId)).Select(brand => new
                        {
                            dataHolderBrandId = brand.BrandId,
                            brandName = brand.BrandName,
                            industries = new List<string> { participation.Industry.IndustryTypeCode.ToUpper() },
                            logoUri = brand.LogoUri,
                            status = brand.BrandStatus.BrandStatusCode,
                            legalEntity = new
                            {
                                legalEntityId = participation.LegalEntity.LegalEntityId,
                                legalEntityName = participation.LegalEntity.LegalEntityName,
                                accreditationNumber = participation.LegalEntity.AccreditationNumber,
                                accreditationLevel = participation.LegalEntity.AccreditationLevel.AccreditationLevelCode.ToUpper(),
                                logoUri = participation.LegalEntity.LogoUri,
                                registrationNumber = participation.LegalEntity.RegistrationNumber,
                                registrationDate = participation.LegalEntity.RegistrationDate.Value.ToString("yyyy-MM-dd"),
                                registeredCountry = participation.LegalEntity.RegisteredCountry,
                                abn = participation.LegalEntity.Abn,
                                acn = participation.LegalEntity.Acn,
                                arbn = participation.LegalEntity.Arbn,
                                anzsicDivision = participation.LegalEntity.AnzsicDivision,
                                organisationType = participation.LegalEntity.OrganisationType.OrganisationTypeCode.ToUpper(),
                                status = participation.Status.ParticipationStatusCode,
                            },
                            endpointDetail = new
                            {
                                version = brand.Endpoint.Version,
                                publicBaseUri = brand.Endpoint.PublicBaseUri,
                                resourceBaseUri = brand.Endpoint.ResourceBaseUri,
                                infosecBaseUri = brand.Endpoint.InfosecBaseUri,
                                extensionBaseUri = brand.Endpoint.ExtensionBaseUri,
                                websiteUri = brand.Endpoint.WebsiteUri

                            },
                            authDetails = new
                            {
                                registerUType = brand.AuthDetails.First().RegisterUType.RegisterUTypeCode,
                                jwksEndpoint = brand.AuthDetails.First().JwksEndpoint
                            }
                        }));

                return JsonConvert.SerializeObject(expectedDataHolder.First().First(), Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting data holder from database for legalEntity: {legalEntityId}, Industry: {industry} \n{ex.Message}", ex);
            }
        }

        private static async Task<string> GetAzureAdAccessToken()
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", AZURE_AD_CLIENT_ID),
                new KeyValuePair<string, string>("scope", AZURE_AD_SCOPE),
                new KeyValuePair<string, string>("client_secret", AZURE_AD_CLIENT_SECRET),
                new KeyValuePair<string, string>("grant_type", AZURE_AD_GRANT_TYPE)
            });

            var response = await client.PostAsync(AZURE_AD_TOKEN_ENDPOINT_URL, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to retrieve access token from Azure AD.\n Http Status Code: {response.StatusCode}\nResponse Body:{responseBody}");
            }

            var tokenResnse = JsonConvert.DeserializeObject<AccessToken>(responseBody);
            return tokenResnse.access_token;
        }

        private static async Task<string> GetInvalidAzureAdAccessToken()
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", AZURE_AD_UNAUTHORISED_CLIENT_ID),
                new KeyValuePair<string, string>("client_secret", AZURE_AD_UNAUTHORISED_CLIENT_SECRET),
                new KeyValuePair<string, string>("scope", AZURE_AD_SCOPE),
                new KeyValuePair<string, string>("grant_type", AZURE_AD_GRANT_TYPE)
            });

            var response = await client.PostAsync(AZURE_AD_TOKEN_ENDPOINT_URL, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unable to retrieve access token from Azure AD.\n Http Status Code: {response.StatusCode}\nResponse Body:{responseBody}");
            }

            var tokenResnse = JsonConvert.DeserializeObject<AccessToken>(responseBody);
            return tokenResnse.access_token;
        }
    }
}
