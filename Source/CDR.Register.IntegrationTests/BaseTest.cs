using CDR.Register.Domain.Models;
using CDR.Register.IntegrationTests.API.Update;
using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        static int count = 0;

        public override void Before(MethodInfo methodUnderTest)
        {
            Log.Information($"********** Test #{++count} - {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name} **********");
            Console.WriteLine($"Test #{++count} - {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
        }

    }

    // Put all tests in same collection because we need them to run sequentially since some tests are mutating DB.
    [Collection("IntegrationTests")]
    [TestCaseOrderer("CDR.Register.IntegrationTests.XUnit.Orderers.AlphabeticalOrderer", "CDR.Register.IntegrationTests")]
    [DisplayTestMethodName]
    abstract public class BaseTest0
    {
    }

    abstract public class BaseTest : BaseTest0, IClassFixture<TestFixture>
    {

        public BaseTest(ITestOutputHelper output, TestFixture testFixture)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .Enrich.WithExceptionDetails()
                .WriteTo.Logger(l =>
                {
                    l.WriteTo.File("AutomationLog.txt");
                })
                .WriteTo.TestOutput(output)
                .CreateLogger();

            JsonConvert.DefaultSettings = () => new CdrJsonSerializerSettings();
            TestFixture=testFixture;
        }


        const string REGISTER_RW = "DefaultConnection";

        static public string CONNECTIONSTRING_REGISTER_RW =>
            ConnectionStringCheck.Check(Configuration.GetConnectionString(REGISTER_RW)
                ?? throw new Exception($"Configuration setting for '{REGISTER_RW}' not found"));

        static private IConfigurationRoot? configuration;
        static public IConfigurationRoot Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                        .AddEnvironmentVariables()
                        .Build();
                }

                return configuration;
            }
        }

        // Client certificates
        protected const string CERTIFICATE_FILENAME = "Certificates/client.pfx";
        protected const string CERTIFICATE_PASSWORD = "#M0ckDataRecipient#";
        protected const string ADDITIONAL_CERTIFICATE_FILENAME = "Certificates/client-additional.pfx";
        protected const string ADDITIONAL_CERTIFICATE_PASSWORD = CERTIFICATE_PASSWORD;
        protected const string DEFAULT_CERTIFICATE_THUMBPRINT = "f0e5146a51f16e236844cf0353d791f11865e405";
        protected const string DEFAULT_CERTIFICATE_COMMON_NAME = "MockDataRecipient";

        // This seed data is copied from ..\CDR.Register.Admin.API\Data\ (see CDR.Register.IntegrationTests.csproj)
        public static readonly string SEEDDATA_FILENAME = $"seed-data.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

        // URLs
        static public string TLS_BaseURL => Configuration["TLS_BaseURL"]
            ?? throw new Exception($"{nameof(TLS_BaseURL)} - configuration setting not found");
        static public string MTLS_BaseURL => Configuration["MTLS_BaseURL"]
            ?? throw new Exception($"{nameof(MTLS_BaseURL)} - configuration setting not found");
        static public string ADMIN_BaseURL => Configuration["Admin_BaseURL"]
            ?? throw new Exception($"{nameof(ADMIN_BaseURL)} - configuration setting not found");
        static public readonly string ADMIN_URL = ADMIN_BaseURL + "/admin/metadata";
        static public readonly string IDENTITYSERVER_URL = MTLS_BaseURL + "/idp/connect/token";

        // START CTS Settings
        static public string AZURE_AD_TOKEN_ENDPOINT_URL => Configuration["CtsSettings:AzureAd:TokenEndpointUrl"] ?? throw new Exception($"{nameof(AZURE_AD_TOKEN_ENDPOINT_URL)} - configuration setting not found");
        static public string AZURE_AD_CLIENT_ID => Configuration["CtsSettings:AzureAd:ClientId"] ?? throw new Exception($"{nameof(AZURE_AD_CLIENT_ID)} - configuration setting not found");
        static public string AZURE_AD_CLIENT_SECRET => Configuration["CtsSettings:AzureAd:ClientSecret"] ?? throw new Exception($"{nameof(AZURE_AD_CLIENT_SECRET)} - configuration setting not found");
        static public string AZURE_AD_UNAUTHORISED_CLIENT_ID => Configuration["CtsSettings:AzureAd:UnauthorisedClientId"] ?? throw new Exception($"{nameof(AZURE_AD_UNAUTHORISED_CLIENT_ID)} - configuration setting not found");
        static public string AZURE_AD_UNAUTHORISED_CLIENT_SECRET => Configuration["CtsSettings:AzureAd:UnauthorisedClientSecret"] ?? throw new Exception($"{nameof(AZURE_AD_UNAUTHORISED_CLIENT_SECRET)} - configuration setting not found");
        static public string AZURE_AD_SCOPE => Configuration["CtsSettings:AzureAd:Scope"] ?? throw new Exception($"{nameof(AZURE_AD_SCOPE)} - configuration setting not found");
        static public string AZURE_AD_GRANT_TYPE => Configuration["CtsSettings:AzureAd:GrantType"] ?? throw new Exception($"{nameof(AZURE_AD_GRANT_TYPE)} - configuration setting not found");
        public const string EXPIRED_ACCESS_TOKEN = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ii1LSTNROW5OUjdiUm9meG1lWm9YcWJIWkdldyJ9.eyJhdWQiOiI3YzVmZmE2Yy1jN2ZhLTRlNDktODMyZi1lZWQ0MzBmODE1MjUiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vYWZiYmE3ZDAtNjc2Zi00MGI3LTkwYmQtOGY0NjM4MDM0YjgyL3YyLjAiLCJpYXQiOjE2ODM3ODIyMTgsIm5iZiI6MTY4Mzc4MjIxOCwiZXhwIjoxNjgzNzg2MTE4LCJhaW8iOiJFMlpnWUdnS2VoSXp6ODFpMWFYazA2NG5jcU15ZU90dUNhOUs4NTB6dGNiYjhvR1g5UVVBIiwiYXpwIjoiMzA5MjJhZWUtMDc5OS00NzkxLWFkNDUtMjI2NTUwZjg2OGJlIiwiYXpwYWNyIjoiMSIsIm9pZCI6ImY0ZGZmMGU2LTQxYjMtNDFlNy1iNWFiLWQ0MzNjZTg4MTY5NiIsInJoIjoiMC5BVUVBMEtlN3IyOW50MENRdlk5R09BTkxnbXo2WDN6NngwbE9neV91MURENEZTVkJBQUEuIiwicm9sZXMiOlsiQXBpLkFkbWluLlBhcnRpY2lwYW50TWV0YURhdGEuV3JpdGUiLCJBcGkuQWRtaW4uUGFydGljaXBhbnRNZXRhRGF0YS5SZWFkIl0sInN1YiI6ImY0ZGZmMGU2LTQxYjMtNDFlNy1iNWFiLWQ0MzNjZTg4MTY5NiIsInRpZCI6ImFmYmJhN2QwLTY3NmYtNDBiNy05MGJkLThmNDYzODAzNGI4MiIsInV0aSI6IlB6SEF3aTlXY2t5dXRkbGRSSkVTQUEiLCJ2ZXIiOiIyLjAifQ.VjMh6-FRMLWAIkYloADH--fTgUVfNgN2XYx3yeJUew1JiCRpiABj4JYkieBxkQ4vrWfj79F3O1ggf2SEOy49nym037CdA3TfW83kpw7MOVHH38-VG-LR_sobAMsS40N4dwNrvsfRQxjQha8gcnskPvtYAWYOII2vfMFxrxAeChwsDGd6A-b5-vo26GyQjebZLcfhMAgu79HFKgIrQRg9MYQ5ZI2wISi2T_d43RYyluXHBtVyCRIfEmUy3aTyJBo6ZHW5omhbUgDp9otwUmwFkv4xrmdrz5ADgqaMelEVllyJrUD9de_wvAh9V5q5Bu6bJhufQoKWXgO-dKIx6baJOA";

        static public string SSA_DOWNSTREAM_BASE_URL => Configuration["SSA_Downstream_BaseUrl"] ?? throw new Exception($"{nameof(SSA_DOWNSTREAM_BASE_URL)} - configuration setting not found");
        static public string DISCOVERY_DOWNSTREAM_BASE_URL => Configuration["Discovery_Downstream_BaseUrl"] ?? throw new Exception($"{nameof(DISCOVERY_DOWNSTREAM_BASE_URL)} - configuration setting not found");
        static public string STATUS_DOWNSTREAM_BASE_URL => Configuration["Status_Downstream_BaseUrl"] ?? throw new Exception($"{nameof(STATUS_DOWNSTREAM_BASE_URL)} - configuration setting not found");
        static public string IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL => Configuration["IdentityProvider_Downstream_BaseUrl"] ?? throw new Exception($"{nameof(IDENTITY_PROVIDER_DOWNSTREAM_BASE_URL)} - configuration setting not found");

        public TestFixture TestFixture { get; }

        //END CTS Settings

        protected const string EXPECTED_UNSUPPORTED_ERROR = @"
            {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                    ""title"": ""Unsupported Version"",
                    ""detail"": ""Requested version is lower than the minimum version or greater than maximum version."",
                    }
                ]
            }";

        protected const string EXPECTED_INVALID_VERSION_ERROR = @"
            {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/InvalidVersion"",
                    ""title"": ""Invalid Version"",
                    ""detail"": ""Version is not a positive Integer.""
                    }
                ]
            }";

        protected const string EXPECTED_MISSING_X_V_ERROR = @"
            {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/Missing"",
                    ""title"": ""Missing Required Header"",
                    ""detail"": ""An API version x-v header is required, but was not specified."",
                    }
                ]
            }";

        /// <summary>
        /// Assert response content and expectedJson are equivalent
        /// </summary>
        /// <param name="expectedJson">The expected json</param>
        /// <param name="content">The response content</param>
        public static async Task Assert_HasContent_Json(string expectedJson, HttpContent content)
        {
            var actualJson = await content.ReadAsStringAsync();
            Log.Information($"Verifying Actual Json:\n{actualJson}\n against expected:\n{expectedJson}");
            Assert_Json(expectedJson, actualJson);
        }

        /// <summary>
        /// Assert actual json is equivalent to expected json
        /// </summary>
        /// <param name="expectedJson">The expected json</param>
        /// <param name="actualJson">The actual json</param>       
        public static void Assert_Json(string expectedJson, string actualJson)
        {
            Log.Information($"Verifying Actual Json:\n{actualJson}\n against expected:\n{expectedJson}");
            //Use Fluentassertions.Json to compare. Whitespace and order is ignored.
            JToken expected = JToken.Parse(expectedJson);
            JToken actual = JToken.Parse(actualJson);
            actual.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Assert headers has a single header with the expected value.
        /// If expectedValue is null then just check for the existence of the header (and not it's value)
        /// </summary>
        /// <param name="expectedValue">The expected header value</param>
        /// <param name="headers">The headers to check</param>
        /// <param name="name">Name of header to check</param>
        public static void Assert_HasHeader(string? expectedValue, HttpHeaders headers, string name)
        {
            headers.Should().NotBeNull();
            if (headers != null)
            {
                // headers.Contains(name).Should().BeTrue($"Header '{name}' is missing");
                headers.Contains(name).Should().BeTrue($"should contain {name} header");
                if (headers.Contains(name))
                {
                    var headerValues = headers.GetValues(name);
                    headerValues.Should().ContainSingle(name, $"Multiple headers with the same name '{name}'");

                    if (expectedValue != null)
                    {
                        var actualValue = headerValues.First();
                        actualValue.Should().Be(expectedValue, $"Expected header '{name}' to be '{expectedValue}' but instead it was '{actualValue}'");
                    }
                }
            }
        }

        /// <summary>
        /// Assert header has content type of ApplicationJson
        /// </summary>
        /// <param name="content"></param>
        public static void Assert_HasContentType_ApplicationJson(HttpContent content)
        {
            content.Should().NotBeNull();
            content?.Headers.Should().NotBeNull();
            content?.Headers?.ContentType.Should().NotBeNull();
            content?.Headers?.ContentType?.ToString().Should().StartWith("application/json");
        }

        /// <summary>
        /// Assert claim exists
        /// </summary>
        public static void AssertClaim(IEnumerable<Claim> claims, string claimType, string claimValue)
        {
            claims.Should().NotBeNull();
            if (claims != null)
            {
                claims.FirstOrDefault(claim => claim.Type == claimType && claim.Value == claimValue).Should().NotBeNull($"Expected {claimType}={claimValue}");
            }
        }

        protected static string GetJsonFromModel<T>(T model)
        {
            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// Get status of SoftwareProduct
        /// </summary>
        public static int GetSoftwareProductStatusId(string softwareProductId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqlCommand("select statusid from softwareproduct where softwareproductid = @softwareproductid", connection);
            selectCommand.Parameters.AddWithValue("@softwareproductid", softwareProductId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Set status of SoftwareProduct
        /// </summary>
        public static void SetSoftwareProductStatusId(string softwareProductId, int statusId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqlCommand("update softwareproduct set statusid = @statusid where softwareproductid = @softwareproductid", connection);
            updateCommand.Parameters.AddWithValue("@softwareproductid", softwareProductId);
            updateCommand.Parameters.AddWithValue("@statusid", statusId);
            updateCommand.ExecuteNonQuery();

            // Check status was updated
            using var selectCommand = new SqlCommand("select count(*) from softwareproduct where softwareproductid = @softwareproductid and statusid = @statusid", connection);
            selectCommand.Parameters.AddWithValue("@softwareproductid", softwareProductId);
            selectCommand.Parameters.AddWithValue("@statusid", statusId);
            if (selectCommand.ExecuteScalarInt32() != 1)
            {
                throw new Exception("Status not updated");
            };
        }

        /// <summary>
        /// Get status of Brand
        /// </summary>
        public static int GetBrandStatusId(string brandId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqlCommand("select brandstatusid from brand where brandid = @brandid", connection);
            selectCommand.Parameters.AddWithValue("@brandid", brandId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Set status of Brand
        /// </summary>
        public static void SetBrandStatusId(string brandId, int statusId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqlCommand("update brand set brandstatusid = @brandstatusid where brandid = @brandid", connection);
            updateCommand.Parameters.AddWithValue("@brandid", brandId);
            updateCommand.Parameters.AddWithValue("@brandstatusid", statusId);
            updateCommand.ExecuteNonQuery();

            // Check status was updated
            using var selectCommand = new SqlCommand("select count(*) from brand where brandid = @brandid and brandstatusid = @brandstatusid", connection);
            selectCommand.Parameters.AddWithValue("@brandid", brandId);
            selectCommand.Parameters.AddWithValue("@brandstatusid", statusId);
            if (selectCommand.ExecuteScalarInt32() != 1)
            {
                throw new Exception("Status not updated");
            };
        }

        /// <summary>
        /// Get participationid for brand
        /// </summary>
        public static string GetParticipationId(string brandId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqlCommand("select participationid from brand where brandid = @brandid", connection);
            selectCommand.Parameters.AddWithValue("@brandid", brandId);

            return selectCommand.ExecuteScalarString(); // presumably brand only has single participation with legal entity, anyway if not this will throw
        }

        /// <summary>
        /// Get status of Participation
        /// </summary>
        public static int GetParticipationStatusId(string participationId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqlCommand("select statusid from participation where participationid = @participationid", connection);
            selectCommand.Parameters.AddWithValue("@participationid", participationId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Get status of Participation
        /// </summary>
        public static void SetParticipationStatusId(string participationId, int statusId)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqlCommand("update participation set statusid = @statusid where participationid = @participationid", connection);
            updateCommand.Parameters.AddWithValue("@participationid", participationId);
            updateCommand.Parameters.AddWithValue("@statusid", statusId);
            updateCommand.ExecuteNonQuery();

            // Check status was updated
            using var selectCommand = new SqlCommand("select count(*) from participation where participationid = @participationid and statusid = @statusid", connection);
            selectCommand.Parameters.AddWithValue("@participationid", participationId);
            selectCommand.Parameters.AddWithValue("@statusid", statusId);
            if (selectCommand.ExecuteScalarInt32() != 1)
            {
                throw new Exception("Status not updated");
            };
        }
        protected static void VerifyParticipationRecord(string legalEntiryId, string expectedParticipationType, string expectedIndustryType, string expectedStatus)
        {
            using SqlConnection registerConnection = new SqlConnection(BaseTest.CONNECTIONSTRING_REGISTER_RW);

            bool singleRow;
            try
            {
                registerConnection.Open();

                string sqlCommand = @$"
                    SELECT 
                        p.ParticipationId,
                        pt.ParticipationTypeCode,
                        i.IndustryTypeCode,
                        ps.ParticipationStatusCode 
                    FROM 
                        Participation p 
	                    FULL JOIN ParticipationType pt ON p.ParticipationTypeId = pt.ParticipationTypeId
	                    FULL JOIN IndustryType i ON p.IndustryId = i.IndustryTypeId
				        FULL JOIN ParticipationStatus ps ON p.StatusId = ps.ParticipationStatusId
                    WHERE 
                        p.LegalEntityId = '{legalEntiryId}'
                        AND pt.ParticipationTypeCode = '{expectedParticipationType}' ";

                if (expectedIndustryType == null)
                {
                    sqlCommand += "AND i.IndustryTypeCode IS NULL";
                }
                else
                {
                    sqlCommand += $"AND i.IndustryTypeCode = '{expectedIndustryType}'";
                }

                using SqlCommand selectCommand = new SqlCommand(sqlCommand, registerConnection);

                using SqlDataReader sqlDataReader = selectCommand.ExecuteReader();

                singleRow = sqlDataReader.Read();

                using (new AssertionScope())
                {
                    sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("ParticipationId")).Should().NotBeEmpty();
                    sqlDataReader.GetString(sqlDataReader.GetOrdinal("ParticipationTypeCode")).Should().Be(expectedParticipationType);
                    sqlDataReader.GetString(sqlDataReader.GetOrdinal("ParticipationStatusCode")).Should().Be(expectedStatus);
                    singleRow.Should().BeTrue(because: "Only one participation record is expceted");

                    if (expectedIndustryType != null)
                    {
                        if (!sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("IndustryTypeCode")))
                        {
                            sqlDataReader.GetString(sqlDataReader.GetOrdinal("IndustryTypeCode")).Should().Be(expectedIndustryType);
                        }
                        else
                        {
                            throw new Exception($"Expected Industry Type of {expectedIndustryType} but found null");
                        }
                    }
                    else
                    {
                        sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("IndustryTypeCode")).Should().BeTrue();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error Getting Participation Record for Legal Entity: {legalEntiryId}", e);
            }
        }

        protected static void VerifyBrandLastUpdatedDateRecord(string legalEntiryId)
        {
            var brands = GetActualBrandsFromDatabase(legalEntiryId);

            // Iterate through all brands and check the LastUpdate date on the Brand record.
            // A 120 second tolerance has been added due to time differences between assertion and the time the record was updated/added.
            foreach (var b in brands)
            {
                Log.Information($"Verifying LastUpdate Brand with Id: '{b.BrandId}' is within 120 seconds from '{DateTime.UtcNow}'");
                b.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(120), because: $"Brand with Id: '{b.BrandId}' was just updated/added.");
            }

        }

        protected static ICollection<Repository.Entities.Brand> GetActualBrandsFromDatabase(string legalEntityId)
        {

            var legalEntityIdGuid = Guid.Parse(legalEntityId);

            try
            {
                using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

                var brands =
                    dbContext.Participations.AsNoTracking()
                        .Include(participation => participation.LegalEntity)
                        .Include(participation => participation.Brands)
                        .Where(participation => participation.LegalEntityId == legalEntityIdGuid)
                        .SelectMany(participation => participation.Brands)
                        .ToArray();
                return brands;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting brands - {ex.Message}");
            }
        }

        protected static async Task VerifyBadRequest(ExpectedErrors expectedErrors, HttpResponseMessage httpResponseFromRegister)
        {
            Log.Information("Expected error response: \n{JsonModel}", GetJsonFromModel(expectedErrors));

            using (new AssertionScope())
            {
                //Check status code
                httpResponseFromRegister.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                string actualErrors = await httpResponseFromRegister.Content.ReadAsStringAsync();

                Assert_Json(GetJsonFromModel(expectedErrors), actualErrors);
            }
        }

        protected static async Task VerifyNotAcceptableRequest(ExpectedErrors expectedErrors, HttpResponseMessage httpResponseFromRegister)
        {
            Log.Information("Expected error response: \n{JsonModel}", GetJsonFromModel(expectedErrors));

            using (new AssertionScope())
            {
                //Check status code
                httpResponseFromRegister.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

                string actualErrors = await httpResponseFromRegister.Content.ReadAsStringAsync();

                Assert_Json(GetJsonFromModel(expectedErrors), actualErrors);
            }
        }

        protected static async Task VerifyUnauthorisedRequest(HttpResponseMessage httpResponseFromRegister)
        {
            var expectedErrorJson = "{\"error\":\"invalid_token\"}";
            Log.Information("Expected error response: \n{ExpectedErrorJson}", expectedErrorJson);

            using (new AssertionScope())
            {
                //Check status code
                httpResponseFromRegister.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

                string actualErrors = await httpResponseFromRegister.Content.ReadAsStringAsync();

                Assert_Json(expectedErrorJson, actualErrors);

            }
        }

        protected static string ConvertJsonPathToPascalCase(string jsonPath)
        {
            string[] allParts = jsonPath.Split(".");

            for (int i = 0; i < allParts.Length; i++)
            {
                allParts[i] = char.ToUpper(allParts[i][0]) + allParts[i].Substring(1);
            }

            return string.Join(".", allParts);
        }

        protected static T? RemoveModelElementBasedOnJsonPath<T>(T model, string path)
        {
            JToken mainJoken = JObject.FromObject(model ?? throw new ArgumentNullException(nameof(model)));

            //If an array element, remove element.
            if (path.Last() == ']')
            {
                JToken token = mainJoken.SelectToken(path, true) ?? throw new Exception("mainJoken cannot be null.");
                token.Remove();
                mainJoken.RemoveEmptyArrays();
            }
            else
            {
                mainJoken.RemovePath(path);
            }

            return mainJoken.ToObject<T>();

        }

        protected static T? ReplaceModelValueBasedOnJsonPath<T>(T model, string path, string newValue)
        {
            JToken mainJoken = JObject.FromObject(model ?? throw new ArgumentNullException(nameof(model)));

            JToken tokenToUpdate = mainJoken.SelectToken(path) ?? throw new Exception($"tokenToUpdate is null. Ensure json path '{path}' exists."); ;

            tokenToUpdate.Replace(newValue);

            return mainJoken.ToObject<T>();

        }
        public static string GenerateDynamicCtsUrl(string baseUrl, string? conformanceId = null)
        {
            if (conformanceId == null)
            {
                return $"{baseUrl}/cts/{Guid.NewGuid()}/register";
            }
            else
            {
                return $"{baseUrl}/cts/{conformanceId}/register";
            }
        }

        protected static string ReplaceSecureHostName(string url, string hostNamedToReplace)
        {
            string secureHostname = Configuration["SecureHostName"] ?? "";

            if (String.IsNullOrEmpty(secureHostname))
            {
                return url;
            }
            else
            {
                return url.Replace(hostNamedToReplace, secureHostname);
            }
        }

        protected static string ReplacePublicHostName(string url, string hostNamedToReplace)
        {
            string publicHostname = Configuration["PublicHostName"] ?? "";

            if (String.IsNullOrEmpty(publicHostname))
            {
                return url;
            }
            else
            {
                return url.Replace(hostNamedToReplace, publicHostname);
            }
        }

    }
}