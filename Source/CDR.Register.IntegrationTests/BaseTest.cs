using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.IntegrationTests.Fixtures;
using CDR.Register.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    // Put all tests in same collection because we need them to run sequentially since some tests are mutating DB.
    // Use SeedDatabaseFixture to seed database prior to running tests.
    [Collection("IntegrationTests")]
    abstract public class BaseTest : IClassFixture<SeedDatabaseFixture>
    {

        private IConfiguration _configuration;

        public BaseTest()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .Build();
        }

        public IConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        // Client certificates
        protected const string CERTIFICATE_FILENAME = "certificates\\client.pfx";
        protected const string CERTIFICATE_PASSWORD = "#M0ckDataRecipient#";
        protected const string ADDITIONAL_CERTIFICATE_FILENAME = "certificates\\client-additional.pfx";
        protected const string ADDITIONAL_CERTIFICATE_PASSWORD = CERTIFICATE_PASSWORD;

        public const string SEEDDATA_FILENAME = "Data\\seed-data.json";

        // URLs
        public const string TLS_BaseURL = "https://localhost:7000";
        public const string MTLS_BaseURL = "https://localhost:7001";
        public const string ADMIN_URL = "https://localhost:7006/admin/metadata";
        public const string IDENTITYSERVER_URL = MTLS_BaseURL + "/idp/connect/token";

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
            //var expectedObject = JsonSerializer.Deserialize<object>(expectedJson);
            var expectedObject = JsonConvert.DeserializeObject<object>(expectedJson);

            //var actualObject = JsonSerializer.Deserialize<object>(actualJson);
            var actualObject = JsonConvert.DeserializeObject<object>(actualJson);

            //var expectedJsonNormalised = JsonSerializer.Serialize(expectedObject);
            var expectedJsonNormalised = JsonConvert.SerializeObject(expectedObject);

            //var actualJsonNormalised = JsonSerializer.Serialize(actualObject);
            var actualJsonNormalised = JsonConvert.SerializeObject(actualObject);

            // actualJsonNormalised.Should().Be(expectedJsonNormalised, because);

            // var actual = JToken.Parse(actualJson);
            // var expected = JToken.Parse(expectedJson);
            // actual.Should().BeEquivalentTo(expected, because);

            expectedJson.Should().NotBeNull();
            actualJson.Should().NotBeNull();
            actualJson?.JsonCompare(expectedJson).Should().BeTrue(
                $"\r\nExpected json:\r\n{expectedJsonNormalised}\r\nActual Json:\r\n{actualJsonNormalised}\r\n"
            );
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
                headers.Contains(name).Should().BeTrue($"Header '{name}' is missing");
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
        public int GetSoftwareProductStatusId(string softwareProductId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqliteCommand("select statusid from softwareproduct where softwareproductid = @softwareproductid", connection);
            selectCommand.Parameters.AddWithValue("@softwareproductid", softwareProductId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Set status of SoftwareProduct
        /// </summary>
        public void SetSoftwareProductStatusId(string softwareProductId, int statusId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqliteCommand("update softwareproduct set statusid = @statusid where softwareproductid = @softwareproductid", connection);
            updateCommand.Parameters.AddWithValue("@softwareproductid", softwareProductId);
            updateCommand.Parameters.AddWithValue("@statusid", statusId);
            updateCommand.ExecuteNonQueryAsync();

            // Check status was updated
            using var selectCommand = new SqliteCommand("select count(*) from softwareproduct where softwareproductid = @softwareproductid and statusid = @statusid", connection);
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
        public int GetBrandStatusId(string brandId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqliteCommand("select brandstatusid from brand where brandid = @brandid", connection);
            selectCommand.Parameters.AddWithValue("@brandid", brandId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Set status of Brand
        /// </summary>
        public void SetBrandStatusId(string brandId, int statusId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqliteCommand("update brand set brandstatusid = @brandstatusid where brandid = @brandid", connection);
            updateCommand.Parameters.AddWithValue("@brandid", brandId);
            updateCommand.Parameters.AddWithValue("@brandstatusid", statusId);
            updateCommand.ExecuteNonQueryAsync();

            // Check status was updated
            using var selectCommand = new SqliteCommand("select count(*) from brand where brandid = @brandid and brandstatusid = @brandstatusid", connection);
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
        public string GetParticipationId(string brandId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqliteCommand("select participationid from brand where brandid = @brandid", connection);
            selectCommand.Parameters.AddWithValue("@brandid", brandId);

            return selectCommand.ExecuteScalarString(); // presumably brand only has single participation with legal entity, anyway if not this will throw
        }

        /// <summary>
        /// Get status of Participation
        /// </summary>
        public int GetParticipationStatusId(string participationId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            using var selectCommand = new SqliteCommand("select statusid from participation where participationid = @participationid", connection);
            selectCommand.Parameters.AddWithValue("@participationid", participationId);

            return selectCommand.ExecuteScalarInt32();
        }

        /// <summary>
        /// Get status of Participation
        /// </summary>
        public void SetParticipationStatusId(string participationId, int statusId)
        {
            using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();

            // Update status
            using var updateCommand = new SqliteCommand("update participation set statusid = @statusid where participationid = @participationid", connection);
            updateCommand.Parameters.AddWithValue("@participationid", participationId);
            updateCommand.Parameters.AddWithValue("@statusid", statusId);
            updateCommand.ExecuteNonQueryAsync();

            // Check status was updated
            using var selectCommand = new SqliteCommand("select count(*) from participation where participationid = @participationid and statusid = @statusid", connection);
            selectCommand.Parameters.AddWithValue("@participationid", participationId);
            selectCommand.Parameters.AddWithValue("@statusid", statusId);
            if (selectCommand.ExecuteScalarInt32() != 1)
            {
                throw new Exception("Status not updated");
            };
        }
    }

}
