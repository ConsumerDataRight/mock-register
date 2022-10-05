using CDR.Register.IntegrationTests.Extensions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        static int count = 0;

        public override void Before(MethodInfo methodUnderTest)
        {
            Console.WriteLine($"Test #{++count} - {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
        }

        // public override void After(MethodInfo methodUnderTest)
        // {
        // }
    }

    // Put all tests in same collection because we need them to run sequentially since some tests are mutating DB.
    [Collection("IntegrationTests")]
    [TestCaseOrderer("CDR.Register.IntegrationTests.XUnit.Orderers.AlphabeticalOrderer", "CDR.Register.IntegrationTests")]
    [DisplayTestMethodName]
    abstract public class BaseTest0
    {
    }

    // Put all tests in same collection because we need them to run sequentially since some tests are mutating DB.
    // [Collection("IntegrationTests")]
    // [TestCaseOrderer("CDR.Register.IntegrationTests.XUnit.Orderers.AlphabeticalOrderer", "CDR.Register.IntegrationTests")]
    // [DisplayTestMethodName]
    // abstract public class BaseTest : IClassFixture<TestFixture>
    abstract public class BaseTest : BaseTest0, IClassFixture<TestFixture>
    {
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

        // This seed data is copied from ..\CDR.Register.Admin.API\Data\ (see CDR.Register.IntegrationTests.csproj)
        public static string SEEDDATA_FILENAME = $"seed-data.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

        // URLs
        static public string TLS_BaseURL => Configuration["TLS_BaseURL"]
            ?? throw new Exception($"{nameof(TLS_BaseURL)} - configuration setting not found");
        static public string MTLS_BaseURL => Configuration["MTLS_BaseURL"]
            ?? throw new Exception($"{nameof(MTLS_BaseURL)} - configuration setting not found");
        static public string ADMIN_BaseURL => Configuration["Admin_BaseURL"]
            ?? throw new Exception($"{nameof(ADMIN_BaseURL)} - configuration setting not found");

        static public string ADMIN_URL = ADMIN_BaseURL + "/admin/metadata";
        static public string IDENTITYSERVER_URL = MTLS_BaseURL + "/idp/connect/token";

        protected const string EXPECTEDCONTENT_INVALIDXV = @"
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


        protected const string EXPECTEDCONTENT_UNSUPPORTEDXV = @"
            {
                ""errors"": [
                    {
                    ""code"": ""urn:au-cds:error:cds-all:Header/UnsupportedVersion"",
                    ""title"": ""Unsupported Version"",
                    ""detail"": ""minimum version: #{minVersion}, maximum version: #{maxVersion}"",
                    ""meta"": {}
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
            Assert_Json(expectedJson, actualJson);
        }

        /// <summary>
        /// Assert actual json is equivalent to expected json
        /// </summary>
        /// <param name="expectedJson">The expected json</param>
        /// <param name="actualJson">The actual json</param>       
        public static void Assert_Json(string expectedJson, string actualJson)
        {
            // Remove formatting from expectedJson
            var expectedObject = JsonConvert.DeserializeObject<object>(expectedJson);
            expectedJson = JsonConvert.SerializeObject(expectedObject, new JsonSerializerSettings { Formatting = Formatting.None });

            // Remove formatting from actualJson
            var actualObject = JsonConvert.DeserializeObject<object>(actualJson);
            actualJson = JsonConvert.SerializeObject(actualObject, new JsonSerializerSettings { Formatting = Formatting.None });

            // Compare
            actualJson.Should().Be(expectedJson);
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
    }
}