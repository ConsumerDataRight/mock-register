using CDR.Register.API.Infrastructure.Models;
using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.IntegrationTests.Models;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CDR.Register.IntegrationTests.API.Update
{
    public class US50480_UpdateDataRecipients : BaseTest
    {
        public US50480_UpdateDataRecipients(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }

        private const string UPDATE_DATA_RECIPIENT_CURRENT_API_VERSION = "1";
        private const string DEFAULT_SCOPES = "openid profile bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:payees:read bank:regular_payments:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.usage:read energy:electricity.der:read energy:accounts.basic:read energy:accounts.basic:read energy:accounts.detail:read " +
                                              "energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:accounts.concessions:read energy:billing:read common:customer.basic:read common:customer.detail:read cdr:registration cdr-register:read";

        private const string DEFAULT_EXISTING_LEGAL_ENTITY_ID = "9d34ede4-2c76-4ecc-a31e-ea8392d31cc9";
        private const string DATA_RECIPIENT_PARTICIPATION_TYPE = "DR";

        private const string TEST_DATA_BASE_URI = "https://TestAumationLogoUri.gov.au";
        private const string TEST_DATA_THUMBPRINT = "52ec9233a5fcb690de8582c18223b4fee2fe3989";

        [Fact]
        public async Task AC02_Add_New_Legal_Entity_With_All_Fields_Fields_Http_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(true);

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);
        }

        [Fact]
        public async Task AC03_UpdateExistingLegalEntity_Http_200()
        {
            // Generate valid payload
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            //Using the same Legal Entity, update some fields.
            dataRecipientMetadata.LegalEntityName = "Updated Legal Entity Name";
            dataRecipientMetadata.DataRecipientBrands.First().BrandName = "Updated Brand Name";
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductName = "Updated Software Product Name";

            // Send Updated Data Recipient to Register
            var updateResponse = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, updateResponse);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);
        }

        [Fact]
        public async Task AC04_AC07_Add_New_Legal_Entity_With_Multiple_DataRecipient_Brands_Http_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Add second Data Recipient
            dataRecipientMetadata.DataRecipientBrands.Add(new DataRecipientMetadata.DataRecipientBrand()
            {
                DataRecipientBrandId = Guid.NewGuid().ToString(),
                BrandName = "Test Automation Data Recipient Brand Name 002",
                LogoUri = $"{TEST_DATA_BASE_URI}/DRBlogo2.png",
                Status = "ACTIVE",
            });

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);
        }

        [Fact]
        public async Task AC05_Add_Brand_To_Existing_Legal_Entiry_Http_200()
        {
            // Generate valid payload
            DataRecipientMetadata originalDataRecipient = GenerateValidDataRecipient(false);

            // Send to Register
            _ = await PostUpdateDataRecipientRequest(GetJsonFromModel(originalDataRecipient));


            //Using the same Legal Entity, Add
            DataRecipientMetadata newDataRecipientMetadata = GetCopyOfDataRecipient(originalDataRecipient);
            newDataRecipientMetadata.DataRecipientBrands.First().DataRecipientBrandId = Guid.NewGuid().ToString();
            newDataRecipientMetadata.DataRecipientBrands.First().BrandName = "Brand Name 2";
            newDataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductId = Guid.NewGuid().ToString();
            newDataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductName = "Software Product Name 2";

            // Send Updated Data Recipient to Register
            var updateResponse = await PostUpdateDataRecipientRequest(GetJsonFromModel(newDataRecipientMetadata));

            // Assert http response and database updates.
            DataRecipientMetadata expectedDataRecipientMetadata = GetCopyOfDataRecipient(originalDataRecipient);
            expectedDataRecipientMetadata.DataRecipientBrands.Add(newDataRecipientMetadata.DataRecipientBrands.First());
            VerifySuccessfulDataRecipientUpdate(expectedDataRecipientMetadata, updateResponse);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(expectedDataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, expectedDataRecipientMetadata.Status);
        }

        [Fact]
        public async Task AC08_Add_Multiple_Duplicate_Certificates()
        {
            // Generate valid payload without a Software Product
            DataRecipientMetadata dataRecipient = GenerateValidDataRecipient(false);

            //Add Duplicate Cert
            dataRecipient.DataRecipientBrands.First().SoftwareProducts.First().Certificates.Add(new DataRecipientMetadata.Certificate()
            {
                CommonName = "Test Automation Certificate Common Name",
                Thumbprint = TEST_DATA_THUMBPRINT
            });

            // Send Updated Data Recipient to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipient));

            // Remove one of the certificates from expected results
            dataRecipient.DataRecipientBrands.First().SoftwareProducts.First().Certificates.Remove(dataRecipient.DataRecipientBrands.First().SoftwareProducts.First().Certificates.First());

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipient, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipient.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipient.Status);
        }


        [Fact]
        public async Task ACXX_Add_New_Legal_Entity_With_Multiple_Software_Products_Http_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Add second Software Product
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(new DataRecipientMetadata.SoftwareProduct()
            {
                SoftwareProductId = Guid.NewGuid().ToString(),
                SoftwareProductName = "Test Automation Data Recipient Brand Name 001 - Software Product 002",
                SoftwareProductDescription = "Test Automation Data Recipient Brand Name 001 - Software Product Description 002",
                LogoUri = $"{TEST_DATA_BASE_URI}/Splogo2.png",
                TosUri = $"{TEST_DATA_BASE_URI}/TosUri2",
                ClientUri = $"{TEST_DATA_BASE_URI}/ClientUri2",
                RecipientBaseUri = TEST_DATA_BASE_URI,
                RevocationUri = $"{TEST_DATA_BASE_URI}/RevocationUri2",
                RedirectUris = new List<string> { $"{TEST_DATA_BASE_URI}/RedirectUris2" },
                JwksUri = $"{TEST_DATA_BASE_URI}/jwksUri2",
                Status = "ACTIVE",
                Certificates = new List<DataRecipientMetadata.Certificate>
                            {
                                new DataRecipientMetadata.Certificate()
                                {
                                    CommonName = "Test Automation Certificate Common Name",
                                    Thumbprint = TEST_DATA_THUMBPRINT
                                }
                            }
            });

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            dataRecipientMetadata.DataRecipientBrands.ToList().OrderBy(b => b.DataRecipientBrandId);

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);
        }

        [Fact]
        public async Task AC10_Scope_Not_Specified_Http_200()
        {
            // Generate valid payload
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            //Remove Scope(s)
            dataRecipientMetadata.DataRecipientBrands.ForEach(dataRecipientBrand =>
            {
                dataRecipientBrand.SoftwareProducts.ForEach(softwareProduct =>
                {
                    softwareProduct.Scope = null;
                });
            });

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            //Update expected Data Reciepent to use default scopes.
            dataRecipientMetadata.DataRecipientBrands.ForEach(dataRecipientBrand =>
            {
                dataRecipientBrand.SoftwareProducts.ForEach(softwareProduct =>
                {
                    softwareProduct.Scope ??= DEFAULT_SCOPES;
                });
            });

            Log.Information($"Expected Data Recipient with default scopes:\n {GetJsonFromModel(dataRecipientMetadata)}");

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);

        }

        [Fact]
        public async Task AC11_Add_New_Legal_Entity_With_Only_Mandatory_Fields_Http_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);

        }

        [Fact]
        public async Task AC11_Add_New_Legal_Entity_With_Only_Mandatory_Fields_And_Nil_Software_Products_Http_200()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Clear();

            // Send to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert http response and database updates.
            VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);

            // Assert Participation Record created correctly
            VerifyParticipationRecord(dataRecipientMetadata.LegalEntityId, DATA_RECIPIENT_PARTICIPATION_TYPE, null, dataRecipientMetadata.Status);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AC12_Missing_Version_In_Header_Http_400(string xv)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingHeader, "An API version x-v header is required, but was not specified.");

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1.1")]
        public async Task AC13_Invalid_Version_Http_400(string xv)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidVersion, "Version is not a positive Integer.");

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);
        }


        [Theory]
        [InlineData("20")]
        public async Task AC20_Unsupported_Version_Http_400(string xv)
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            // Send to Register with blank x-v header
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata), xv: xv);

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.UnsupportedVersion, "Requested version is lower than the minimum version or greater than maximum version.");

            // Assert Response
            await VerifyNotAcceptableRequest(expectedErrors, response);
        }

        [Theory]
        [InlineData("Missing Legal Entity Id", "legalEntityId")]
        [InlineData("Missing Legal Entity Name", "legalEntityName")]
        [InlineData("Missing Acreditation Number", "accreditationNumber")]
        [InlineData("Missing Acreditation Level", "accreditationLevel", true)]
        [InlineData("Missing Logo Uri", "logoUri")]
        [InlineData("Missing Status", "status", true)]
        [InlineData("Missing Data Recipient Brands", "dataRecipientBrands")]
        [InlineData("Missing Data Recipient Brand - Brand Id", "dataRecipientBrands[0].dataRecipientBrandId")]
        [InlineData("Missing Data Recipient Brand - Brand Name", "dataRecipientBrands[0].brandName")]
        [InlineData("Missing Data Recipient Brand - Logo Uri", "dataRecipientBrands[0].logoUri")]
        [InlineData("Missing Data Recipient Brand - Status", "dataRecipientBrands[0].status", true)]
        [InlineData("Missing Data Software Product - Id", "dataRecipientBrands[0].softwareProducts[0].softwareProductId")]
        [InlineData("Missing Data Software Product - Name", "dataRecipientBrands[0].softwareProducts[0].softwareProductName")]
        [InlineData("Missing Data Software Product - Description", "dataRecipientBrands[0].softwareProducts[0].softwareProductDescription")]
        [InlineData("Missing Data Software Product - Logo Uri", "dataRecipientBrands[0].softwareProducts[0].logoUri")]
        [InlineData("Missing Data Software Product - Client Uri", "dataRecipientBrands[0].softwareProducts[0].clientUri")]
        [InlineData("Missing Data Software Product - Recipient Base Uri", "dataRecipientBrands[0].softwareProducts[0].recipientBaseUri")]
        [InlineData("Missing Data Software Product - Revocation Uri", "dataRecipientBrands[0].softwareProducts[0].revocationUri")]
        [InlineData("Missing Data Software Product - Redirect Uris", "dataRecipientBrands[0].softwareProducts[0].redirectUris")]
        [InlineData("Missing Data Software Product - Jwks Uri", "dataRecipientBrands[0].softwareProducts[0].jwksUri")]
        [InlineData("Missing Data Software Product - Status", "dataRecipientBrands[0].softwareProducts[0].status", true)]
        [InlineData("Missing Data Software Product - Certificates", "dataRecipientBrands[0].softwareProducts[0].certificates")]
        [InlineData("Missing Data Software Product - Certificate Common Name", "dataRecipientBrands[0].softwareProducts[0].certificates[0].commonName")]
        [InlineData("Missing Data Software Product - Certificates Thumbprint", "dataRecipientBrands[0].softwareProducts[0].certificates[0].thumbprint")]
        public async Task AC14_Missing_Mandatory_Fields_Http_400(string testId, string elementToRemove, bool expectEnumErrorAsWell = false)
        {
            Log.Information($"Executing test for: {testId}");

            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            dataRecipientMetadata = RemoveModelElementBasedOnJsonPath(dataRecipientMetadata, elementToRemove);
            Log.Information($"Modified Payload (element removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            string expectedMissingField = ConvertJsonPathToPascalCase(elementToRemove);

            ExpectedErrors expectedErrors = new();

            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, $"{expectedMissingField}");

            // Multiple errors when when a mandatory enum is blank.
            if (expectEnumErrorAsWell)
            {
                expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, $"Value '' is not allowed for {ConvertJsonPathToPascalCase(expectedMissingField.GetLastFieldFromJsonPath())}");
            }

            // Send Request to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);

        }

        [Fact]
        public async Task AC14_Missing_Multiple_Mandatory_Fields_Http_400()
        {

            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Remove mandatory fields
            dataRecipientMetadata.AccreditationNumber = "";
            dataRecipientMetadata.LegalEntityName = null;
            dataRecipientMetadata.DataRecipientBrands.First().LogoUri = "";
            dataRecipientMetadata.DataRecipientBrands.First().BrandName = null;

            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            ExpectedErrors expectedErrors = new();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, "LegalEntityName");
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, "AccreditationNumber");
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, "DataRecipientBrands[0].BrandName");
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.MissingField, "DataRecipientBrands[0].LogoUri");

            // Send Request to Register
            var response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            // Assert Response
            await VerifyBadRequest(expectedErrors, response);

        }

        [Theory]
        [InlineData("UNRESTRICTED")]
        [InlineData("SPONSORED")]
        [InlineData("sponsored")]
        [InlineData("foo", false)]
        public async Task AC15_Valid_And_Invalid_Data_Recipient_Acreditation_Level(string accreditationLevel, bool isValid = true)
        {
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Modify AccreditationLevel
            dataRecipientMetadata.AccreditationLevel = accreditationLevel;
            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            dataRecipientMetadata.AccreditationLevel = dataRecipientMetadata.AccreditationLevel.ToUpper();

            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, "AccreditationLevel", accreditationLevel, isValid);

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
        public async Task AC15_Valid_And_Invalid_Data_Recipient_Organisation_Type(string orgType, bool isValid = true)
        {
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Modify OrganisationType
            dataRecipientMetadata.OrganisationType = orgType;
            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            dataRecipientMetadata.OrganisationType = dataRecipientMetadata.OrganisationType.ToUpper();

            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, "OrganisationType", orgType, isValid);

        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("Active")]
        [InlineData("SUSPENDED")]
        [InlineData("REVOKED")]
        [InlineData("SURRENDERED")]
        [InlineData("INACTIVE")]
        [InlineData("foo", false)]
        public async Task AC15_Valid_And_Invalid_Data_Recipient_Status(string status, bool isValid = true)
        {
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Modify Status
            dataRecipientMetadata.Status = status;
            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            dataRecipientMetadata.Status = dataRecipientMetadata.Status.ToUpper();

            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, "Status", status, isValid);

        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("Active")]
        [InlineData("INACTIVE")]
        [InlineData("REMOVED")]
        [InlineData("foo", false)]
        public async Task AC15_Valid_And_Invalid_Data_Recipient_Brand_Status(string status, bool isValid = true)
        {
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Modify DR Status
            dataRecipientMetadata.DataRecipientBrands.First().Status = status;
            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            dataRecipientMetadata.DataRecipientBrands.First().Status = dataRecipientMetadata.DataRecipientBrands.First().Status.ToUpper();

            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, "Status", status, isValid);

        }

        [Theory]
        [InlineData("ACTIVE")]
        [InlineData("Active")]
        [InlineData("INACTIVE")]
        [InlineData("REMOVED")]
        [InlineData("foo", false)]
        public async Task AC15_Valid_And_Invalid_Data_Recipient_Software_Product_Status(string status, bool isValid = true)
        {
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient();
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Modify DR - SP Status
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().Status = status;
            Log.Information($"Modified Payload (elements removed):\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().Status = dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().Status.ToUpper();

            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, "Status", status, isValid);

        }

        [Fact]
        public async Task AC16_Brand_Already_Associated_With_Different_Legal_Entity()
        {
            // Get existing Data Recipient from DB
            string orginalDataRecipientFromDb = GetActualDataRecipientFromDatabase(DEFAULT_EXISTING_LEGAL_ENTITY_ID);
            DataRecipientMetadata originalDataRecipientMetadata = JsonConvert.DeserializeObject<DataRecipientMetadata>(orginalDataRecipientFromDb);
            Log.Information($"Original from Database:\n{GetJsonFromModel(originalDataRecipientMetadata)}");

            // Modify the LegalEntityId to a new value and keep Data Recipient Brand LegalEntityId unchanged (already linked to orignal LegalEntityId).
            originalDataRecipientMetadata.LegalEntityId = Guid.NewGuid().ToString();

            Log.Information($"Modified from Database:\n{GetJsonFromModel(originalDataRecipientMetadata)}");

            string dataRecipientBrandId = originalDataRecipientMetadata.DataRecipientBrands.First().DataRecipientBrandId;

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(originalDataRecipientMetadata));

            await VerifyInvalidPayloadResponse(response, originalDataRecipientMetadata, $"dataRecipientBrandId '{dataRecipientBrandId}' is already associated with a different legal entity.");

        }

        [Fact]
        public async Task AC17_Software_Product_Already_Associated_With_Dataholder_Brand()
        {
            // Get existing Data Recipient from DB
            string orginalDataRecipientFromDb = GetActualDataRecipientFromDatabase(DEFAULT_EXISTING_LEGAL_ENTITY_ID);
            DataRecipientMetadata originalDataRecipientMetadata = JsonConvert.DeserializeObject<DataRecipientMetadata>(orginalDataRecipientFromDb);
            Log.Information($"original from Database:\n{GetJsonFromModel(originalDataRecipientMetadata)}");

            // Get First Software Product Id.
            string softwareProductId = originalDataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductId;

            // Generate new Data Recipient payload and replace softwareProductId with the already associated softwareProductId  (already linked to orignal Dataholder Brand).
            DataRecipientMetadata generatedDataRecipientMetadata = GenerateValidDataRecipient();
            generatedDataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductId = softwareProductId;

            Log.Information($"Payload to send:\n{GetJsonFromModel(generatedDataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(generatedDataRecipientMetadata));

            await VerifyInvalidPayloadResponse(response, generatedDataRecipientMetadata, $"Value '{softwareProductId}' in SoftwareProductId is already associated with a different brand.");

        }

        [Fact]
        public async Task AC18_Duplicate_Data_Recipient_Brands_Http_400()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);

            string brandId = dataRecipientMetadata.DataRecipientBrands.First().DataRecipientBrandId;

            // Add Data Recipient Brand with duplicate id
            dataRecipientMetadata.DataRecipientBrands.Add(new DataRecipientMetadata.DataRecipientBrand()
            {
                DataRecipientBrandId = brandId,
                BrandName = "Test Automation Data Recipient Brand Name 002",
                LogoUri = $"{TEST_DATA_BASE_URI}/DRBlogo2.png",
                Status = "ACTIVE",
            });

            Log.Information($"Payload to send:\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            await VerifyInvalidPayloadResponse(response, dataRecipientMetadata, $"Duplicate DataRecipientBrandId '{brandId}' is not allowed in the same request");
        }

        [Fact]
        public async Task AC19_Duplicate_Software_Products_Http_400()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            string initialSoftwareProductId = dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SoftwareProductId;

            // Add Software Product with duplicate id
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(GenerateNewSofwareProduct());

            // Set both Software Products to the same id
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.ForEach(sp => sp.SoftwareProductId = initialSoftwareProductId);
            Log.Information($"Payload to send:\n{GetJsonFromModel(dataRecipientMetadata)}");

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            await VerifyInvalidPayloadResponse(response, dataRecipientMetadata, $"Duplicate softwareProductId '{initialSoftwareProductId}' is not allowed in the same request");

        }

        [Fact]
        public async Task AC19_Multiple_Duplicate_Software_Products_Http_400()
        {
            // Generate valid payload with only mandatory/minimun fields.
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(false);
            Log.Information($"Original Payload:\n{GetJsonFromModel(dataRecipientMetadata)}");

            DataRecipientMetadata.SoftwareProduct softwareProduct1 = GenerateNewSofwareProduct();
            DataRecipientMetadata.SoftwareProduct softwareProduct2 = GenerateNewSofwareProduct();
            softwareProduct1.SoftwareProductId = softwareProduct2.SoftwareProductId;

            DataRecipientMetadata.SoftwareProduct softwareProduct3 = GenerateNewSofwareProduct();
            DataRecipientMetadata.SoftwareProduct softwareProduct4 = GenerateNewSofwareProduct();
            softwareProduct3.SoftwareProductId = softwareProduct4.SoftwareProductId;

            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(softwareProduct1);
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(softwareProduct2);
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(softwareProduct3);
            dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.Add(softwareProduct4);

            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));

            await VerifyInvalidPayloadResponse(response, dataRecipientMetadata, $"Duplicate softwareProductId '{softwareProduct1.SoftwareProductId}' is not allowed in the same request");

        }

        [Theory]
        [InlineData("Max Length Legal Entity - Name", "legalEntityName", 200)]
        [InlineData("Max Length Legal Entity - Accreditation Number", "accreditationNumber", 100)]
        [InlineData("Max Length Legal Entity - Logo Uri", "logoUri", 1000)]
        [InlineData("Max Length Legal Entity - Registration Number", "registrationNumber", 100)]
        [InlineData("Max Length Legal Entity - Registered Country", "registeredCountry", 100)]
        [InlineData("Max Length Legal Entity - ABN", "abn", 11)]
        [InlineData("Max Length Legal Entity - ACN", "acn", 9)]
        [InlineData("Max Length Legal Entity - ARBN", "arbn", 9)]
        [InlineData("Max Length Legal Entity - Anzsic Division", "anzsicDivision", 100)]
        [InlineData("Max Length Data Recipient - Brand Name", "dataRecipientBrands[0].brandName", 200)]
        [InlineData("Max Length Data Recipient - Logo Uri", "dataRecipientBrands[0].logoUri", 1000)]
        [InlineData("Max Length Software Product - Name", "dataRecipientBrands[0].softwareProducts[0].softwareProductName", 200)]
        [InlineData("Max Length Software Product - Description", "dataRecipientBrands[0].softwareProducts[0].softwareProductDescription", 4000)]
        [InlineData("Max Length Software Product - Logo Uri", "dataRecipientBrands[0].softwareProducts[0].logoUri", 1000)]
        [InlineData("Max Length Software Product - Sector Identifier Uri", "dataRecipientBrands[0].softwareProducts[0].sectorIdentifierUri", 2048)]
        [InlineData("Max Length Software Product - Client Uri", "dataRecipientBrands[0].softwareProducts[0].clientUri", 1000)]
        [InlineData("Max Length Software Product - Tos Uri", "dataRecipientBrands[0].softwareProducts[0].tosUri", 1000)]
        [InlineData("Max Length Software Product - Policy Uri", "dataRecipientBrands[0].softwareProducts[0].policyUri", 1000)]
        [InlineData("Max Length Software Product - Recipient Base Uri", "dataRecipientBrands[0].softwareProducts[0].recipientBaseUri", 1000)]
        [InlineData("Max Length Software Product - Revocation Uri", "dataRecipientBrands[0].softwareProducts[0].revocationUri", 1000)]
        [InlineData("Max Length Software Product - Jwks Uri", "dataRecipientBrands[0].softwareProducts[0].jwksUri", 1000)]
        [InlineData("Max Length Software Product - Certificate Common Name", "dataRecipientBrands[0].softwareProducts[0].certificates[0].commonName", 2000)]
        [InlineData("Max Length Software Product - Certificates Thumbprint", "dataRecipientBrands[0].softwareProducts[0].certificates[0].thumbprint", 2000)]
        public async Task ACXX_Maximum_Field_Length_Exceeded(string testId, string elementUnderTest, int maxLenght)
        {
            Log.Information($"Executing test for: {testId} : Max Lenght: {maxLenght}");

            // Create dataRecipient using maximum field lenght
            DataRecipientMetadata dataRecipientMetadata = GenerateValidDataRecipient(true);
            string maxLengthValue = maxLenght.GenerateRandomString();
            dataRecipientMetadata = ReplaceModelValueBasedOnJsonPath(dataRecipientMetadata, elementUnderTest, maxLengthValue);
            Log.Information($"+ve Scenario using maximum field lenght of '{maxLenght}':\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Send and verify positive scenario
            HttpResponseMessage response = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));
            await VerifyInvalidAndValidFieldResponse(response, dataRecipientMetadata, ConvertJsonPathToPascalCase(elementUnderTest), maxLengthValue, true);

            // Create dataRecipient using maximum field lenght plus one
            string maxLengthPlusOneValue = (maxLenght + 1).GenerateRandomString();
            dataRecipientMetadata = ReplaceModelValueBasedOnJsonPath(dataRecipientMetadata, elementUnderTest, maxLengthPlusOneValue);
            Log.Information($"-ve:\n{GetJsonFromModel(dataRecipientMetadata)}");

            // Send and verify negative scenario
            HttpResponseMessage responseNegative = await PostUpdateDataRecipientRequest(GetJsonFromModel(dataRecipientMetadata));
            await VerifyInvalidAndValidFieldResponse(responseNegative, dataRecipientMetadata, ConvertJsonPathToPascalCase(elementUnderTest.GetLastFieldFromJsonPath()), maxLengthPlusOneValue, false);

        }


        private static string RemoveEmptyJsonArrays(string json)
        {
            JObject jObject = JsonConvert.DeserializeObject<JObject>(json);
            jObject.RemoveEmptyArrays();
            return jObject.ToString();
        }

        private static DataRecipientMetadata GetCopyOfDataRecipient(DataRecipientMetadata dataRecipientMetadata)
        {
            string dataRecipientJson = GetJsonFromModel(dataRecipientMetadata);
            return JsonConvert.DeserializeObject<DataRecipientMetadata>(dataRecipientJson);
        }

        private static async Task VerifyInvalidAndValidFieldResponse(HttpResponseMessage response, DataRecipientMetadata dataRecipientMetadata, string field, string value, bool isValid)
        {

            if (isValid)
            {
                VerifySuccessfulDataRecipientUpdate(dataRecipientMetadata, response);
            }
            else
            {
                ExpectedErrors expectedErrors = new ExpectedErrors();
                expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, $"Value '{value}' is not allowed for {field}");

                await VerifyBadRequest(expectedErrors, response);
            }
        }

        private static async Task VerifyInvalidPayloadResponse(HttpResponseMessage response, DataRecipientMetadata dataRecipientMetadata, string expectedErrorMessage)
        {

            ExpectedErrors expectedErrors = new ExpectedErrors();
            expectedErrors.AddExpectedError(ExpectedErrors.ErrorType.InvalidField, expectedErrorMessage);

            await VerifyBadRequest(expectedErrors, response);

        }

        private static void VerifySuccessfulDataRecipientUpdate(DataRecipientMetadata expectedDataRecipientMetadata, HttpResponseMessage httpResponseFromRegister)
        {
            using (new AssertionScope())
            {
                //Check status code
                httpResponseFromRegister.StatusCode.Should().Be(HttpStatusCode.OK);

                // For each data recipient, order software products and set default scope to default if missing
                foreach (var dr in expectedDataRecipientMetadata.DataRecipientBrands)
                {
                    if (dr.SoftwareProducts != null)
                    {
                        dr.SoftwareProducts = dr.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();

                        foreach (DataRecipientMetadata.SoftwareProduct sp in dr.SoftwareProducts)
                        {
                            sp.Scope ??= DEFAULT_SCOPES;
                        }
                    }

                }

                expectedDataRecipientMetadata.DataRecipientBrands = expectedDataRecipientMetadata.DataRecipientBrands.OrderBy(b => b.DataRecipientBrandId).ToList();

                string actualDataRecipient = GetActualDataRecipientFromDatabase(expectedDataRecipientMetadata.LegalEntityId);

                string expectedDataRecipientJson = GetJsonFromModel(expectedDataRecipientMetadata);
                expectedDataRecipientJson = RemoveEmptyJsonArrays(expectedDataRecipientJson);

                Assert_Json(expectedDataRecipientJson, actualDataRecipient);

                VerifyBrandLastUpdatedDateRecord(expectedDataRecipientMetadata.LegalEntityId);
            }
        }

        private static async Task<HttpResponseMessage> PostUpdateDataRecipientRequest(string payload, string xv = UPDATE_DATA_RECIPIENT_CURRENT_API_VERSION)
        {

            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var client = new HttpClient(clientHandler);
            //var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:3500/admin/metadata/data-recipients");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ADMIN_BaseURL}/admin/metadata/data-recipients");

            request.Content = new StringContent($"{payload}", Encoding.UTF8, "application/json");

            if (xv != null)
            {
                request.Headers.Add("x-v", xv);
            }

            Log.Information($"Sending payload:\n{payload}");
            Log.Information($"Request Headers:\n {request.Headers}");

            HttpResponseMessage httpResponse = await client.SendAsync(request);
            Log.Information($"Response from admin/metadata/data-recipients API: {httpResponse.StatusCode} \n{httpResponse.Content.ReadAsStringAsync().Result}");

            return httpResponse;

        }

        private static DataRecipientMetadata GenerateValidDataRecipient(bool includeOptionalFields = false)
        {

            DataRecipientMetadata dataRecipientMetadata = new DataRecipientMetadata
            {
                LegalEntityId = Guid.NewGuid().ToString(),
                LegalEntityName = "Test Automation Generated Data Recipient Name",
                AccreditationNumber = "AcrAuto00001",
                AccreditationLevel = "SPONSORED",
                LogoUri = $"{TEST_DATA_BASE_URI}logo.png",
                Status = "ACTIVE",
                DataRecipientBrands = new List<DataRecipientMetadata.DataRecipientBrand>
                {
                    new DataRecipientMetadata.DataRecipientBrand()
                    {
                        DataRecipientBrandId = Guid.NewGuid().ToString(),
                        BrandName = "Test Automation Data Recipient Brand Name 001",
                        LogoUri= $"{TEST_DATA_BASE_URI}/DRBlogo.png",
                        Status = "ACTIVE",
                        SoftwareProducts = new List<DataRecipientMetadata.SoftwareProduct>
                        {
                            GenerateNewSofwareProduct()
                        }
                    }
                }
            };

            if (includeOptionalFields)
            {
                dataRecipientMetadata.RegistrationNumber = "AutoRegistrationNo.0123456789";
                dataRecipientMetadata.RegistrationDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                dataRecipientMetadata.RegisteredCountry = "New Zealand";
                dataRecipientMetadata.Abn = "80000000001";
                dataRecipientMetadata.Acn = "123456789";
                dataRecipientMetadata.Arbn = "987654321";
                dataRecipientMetadata.AnzsicDivision = "Test Automation Anzsic Division";
                dataRecipientMetadata.OrganisationType = "COMPANY";
                dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().SectorIdentifierUri = $"{TEST_DATA_BASE_URI}/SectorIdentifierUri";
                dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().TosUri = $"{TEST_DATA_BASE_URI}/TosUri";
                dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().PolicyUri = $"{TEST_DATA_BASE_URI}/PolicyUri";
                dataRecipientMetadata.DataRecipientBrands.First().SoftwareProducts.First().Scope = "openid profile cdr-register:read";

            }

            return dataRecipientMetadata;

        }
        private static DataRecipientMetadata.SoftwareProduct GenerateNewSofwareProduct()
        {
            DataRecipientMetadata.SoftwareProduct softwareProduct = new()
            {
                SoftwareProductId = Guid.NewGuid().ToString(),
                SoftwareProductName = "Test Automation Data Recipient Brand Name 001 - Software Product 001",
                SoftwareProductDescription = "Test Automation Data Recipient Brand Name 001 - Software Product Description 001",
                LogoUri = $"{TEST_DATA_BASE_URI}/Splogo.png",
                TosUri = $"{TEST_DATA_BASE_URI}/TosUri",
                ClientUri = $"{TEST_DATA_BASE_URI}/ClientUri",
                RecipientBaseUri = TEST_DATA_BASE_URI,
                RevocationUri = $"{TEST_DATA_BASE_URI}/RevocationUri",
                RedirectUris = new List<string> { $"{TEST_DATA_BASE_URI}/RedirectUris" },
                JwksUri = $"{TEST_DATA_BASE_URI}/jwksUri",
                Status = "ACTIVE",
                Certificates = new List<DataRecipientMetadata.Certificate>
                            {
                                new DataRecipientMetadata.Certificate()
                                {
                                    CommonName = "Test Automation Certificate Common Name",
                                    Thumbprint = TEST_DATA_THUMBPRINT
                                }
                            }
            };
            return softwareProduct;
        }

        private static string GetActualDataRecipientFromDatabase(string legalEntityId)
        {

            var legalEntityIdGuid = Guid.Parse(legalEntityId);

            try
            {
                using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

                var expectedDataRecipients =
                    dbContext.Participations.AsNoTracking()
                        .Include(participation => participation.Status)
                        .Include(participation => participation.Industry)
                        .Include(participation => participation.LegalEntity)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.BrandStatus)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.SoftwareProducts)
                        .ThenInclude(softwareProduct => softwareProduct.Status)
                        .Where(participation => participation.LegalEntityId == legalEntityIdGuid)
                        .OrderBy(participation => participation.LegalEntityId)
                        .Select(participation => new
                        {
                            legalEntityId = participation.LegalEntityId,
                            legalEntityName = participation.LegalEntity.LegalEntityName,
                            accreditationNumber = participation.LegalEntity.AccreditationNumber,
                            accreditationLevel = participation.LegalEntity.AccreditationLevel.AccreditationLevelCode.ToUpper(),
                            logoUri = participation.LegalEntity.LogoUri,
                            registrationNumber = participation.LegalEntity.RegistrationNumber,
                            registrationDate = participation.LegalEntity.RegistrationDate.Value.ToString("yyyy-MM-dd"),
                            registeredCountry = participation.LegalEntity.RegisteredCountry,
                            abn = participation.LegalEntity.Abn,
                            acn = participation.LegalEntity.Acn,
                            arbn = participation.LegalEntity.Arbn == "" ? null : participation.LegalEntity.Arbn,
                            anzsicDivision = participation.LegalEntity.AnzsicDivision,
                            organisationType = participation.LegalEntity.OrganisationType.OrganisationTypeCode.ToUpper(),
                            status = participation.Status.ParticipationStatusCode,
                            dataRecipientBrands = participation.Brands.OrderBy(b => b.BrandId.ToString()).Select(brand => new
                            {
                                dataRecipientBrandId = brand.BrandId,
                                brandName = brand.BrandName,
                                logoUri = brand.LogoUri,
                                status = brand.BrandStatus.BrandStatusCode,
                                softwareProducts = brand.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId.ToString()).Select(softwareProduct => new
                                {
                                    softwareProductId = softwareProduct.SoftwareProductId,
                                    softwareProductName = softwareProduct.SoftwareProductName,
                                    softwareProductDescription = softwareProduct.SoftwareProductDescription,
                                    logoUri = softwareProduct.LogoUri,
                                    sectorIdentifierUri = softwareProduct.SectorIdentifierUri,
                                    clientUri = softwareProduct.ClientUri,
                                    tosUri = softwareProduct.TosUri,
                                    policyUri = softwareProduct.PolicyUri,
                                    recipientBaseUri = softwareProduct.RecipientBaseUri,
                                    revocationUri = softwareProduct.RevocationUri,
                                    redirectUris = softwareProduct.RedirectUris.Split(" ", StringSplitOptions.None).ToList(),
                                    jwksUri = softwareProduct.JwksUri,
                                    scope = softwareProduct.Scope,
                                    status = softwareProduct.Status.SoftwareProductStatusCode,
                                    certificates = softwareProduct.Certificates.Select(certificates => new
                                    {
                                        commonName = certificates.CommonName,
                                        thumbprint = certificates.Thumbprint,
                                    })
                                }),
                            }),

                        });


                string jsonRepresentation = JsonConvert.SerializeObject(expectedDataRecipients.First());

                return RemoveEmptyJsonArrays(jsonRepresentation);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting expected data recipients - {ex.Message}");
            }
        }
    }
}
