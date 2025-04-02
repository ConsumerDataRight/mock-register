// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    public class US29068_TemporalTableTests : BaseTest0
    {
        private static async Task<string> GetTableJson(SqlConnection connection, string tableName, DateTime? pointInTimeUTC = null)
        {
            string orderby = tableName.ToUpper() switch
            {
                "AUTHDETAIL" => "BrandID",
                "ENDPOINT" => "BrandID",
                _ => $"{tableName}ID"
            };

            IEnumerable<dynamic>? data;

            string sql = @$"
                select * from {tableName} 
                {(pointInTimeUTC == null ? string.Empty : $"for system_time as of '{pointInTimeUTC:yyyy-MM-dd HH:mm:ss.fffffff}'")}
                order by {orderby}";

            data = await connection.QueryAsync(sql);

            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { });
        }

        private delegate Task Mutate(SqlConnection connection);

        private async Task Test(string tableName, Mutate mutate)
        {
            await Test(new string[] { tableName }, mutate);
        }

        private static async Task Test(string[] tableNames, Mutate mutate)
        {
            // Arrange
            await DatabaseSeeder.Execute();

            using var registerConnection = new SqlConnection(BaseTest.CONNECTIONSTRING_REGISTER_RW);
            await registerConnection.OpenAsync();

            var pointInTimeUTC = DateTime.UtcNow;

            var initialData = new Dictionary<string, string>();
            foreach (string tableName in tableNames)
            {
                initialData.Add(tableName, await GetTableJson(registerConnection, tableName));
            }

            // Act - Mutate data
            await Task.Delay(500);
            if (mutate != null)
            {
                await mutate(registerConnection);
            }

            var modifiedData = new Dictionary<string, string>();
            foreach (string tableName in tableNames)
            {
                modifiedData.Add(tableName, await GetTableJson(registerConnection, tableName));
            }

            // Assert
            var pointInTimeData = new Dictionary<string, string>();
            foreach (string tableName in tableNames)
            {
                pointInTimeData.Add(tableName, await GetTableJson(registerConnection, tableName, pointInTimeUTC));
            }

#if DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON
            foreach (string tableName in tableNames)
            {
                File.WriteAllText($"c:/temp/{tableName}_initial.json", initialData[tableName]);
                File.WriteAllText($"c:/temp/{tableName}_modified.json", modifiedData[tableName]);
                File.WriteAllText($"c:/temp/{tableName}_pointintime.json", pointInTimeData[tableName]);
            }
#endif

            using (new AssertionScope())
            {
                // Check data was actually modified
                foreach (string tableName in tableNames)
                {
                    modifiedData[tableName].Should().NotBe(initialData[tableName]);
                }

                // Check point in time data matches initial data
                foreach (string tableName in tableNames)
                {
                    pointInTimeData[tableName].Should().Be(initialData[tableName]);
                }
            }
        }

        // [InlineData("AuthDetail")] // One-to-one for brand, ie can't insert another row for existing brand
        // [InlineData("Endpoint")] // One-to-one for brand, ie can't insert another row for existing brand
        [Theory]
        [InlineData("LegalEntity")]
        [InlineData("Participation")]
        [InlineData("Brand")]
        [InlineData("SoftwareProduct")]
        [InlineData("SoftwareProductCertificate")]
        public async Task ACX01_AfterInsertingRecord_PointInTimeQueryShouldNotReturnInsertedRecord(string tableName)
        {
            await Test(tableName, async (connection) =>
            {
                var insert = tableName switch
                {
                    "LegalEntity" => connection.InsertAsync(new LegalEntity()),
                    "Participation" => connection.InsertAsync(new Participation()
                    {
                        LegalEntityId = await connection.ExecuteScalarAsync<Guid>("select top 1 legalentityid from legalentity")
                    }),
                    "Brand" => connection.InsertAsync(new Brand()
                    {
                        ParticipationId = await connection.ExecuteScalarAsync<Guid>("select top 1 participationid from participation")
                    }),
                    "AuthDetail" => connection.InsertAsync(new AuthDetail()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand")
                    }),
                    "Endpoint" => connection.InsertAsync(new Endpoint()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand")
                    }),
                    "SoftwareProduct" => connection.InsertAsync(new SoftwareProduct()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand")
                    }),
                    "SoftwareProductCertificate" => connection.InsertAsync(new SoftwareProductCertificate()
                    {
                        SoftwareProductId = await connection.ExecuteScalarAsync<Guid>("select top 1 softwareProductid from softwareProduct")
                    }),
                    _ => throw new NotSupportedException()
                };
                await insert;
            });
        }

        [Fact]
        public async Task ACX03_AfterDeletingRecords_PointInTimeQueryShouldReturnDeletedRecords()
        {
            await Test(
                ["LegalEntity", "Participation", "Brand", "AuthDetail", "Endpoint", "SoftwareProduct", "SoftwareProductCertificate"],
                async (connection) =>
                {
                    await connection.ExecuteAsync("delete SoftwareProductCertificate");
                    await connection.ExecuteAsync("delete SoftwareProduct");
                    await connection.ExecuteAsync("delete Endpoint");
                    await connection.ExecuteAsync("delete AuthDetail");
                    await connection.ExecuteAsync("delete Brand");
                    await connection.ExecuteAsync("delete Participation");
                    await connection.ExecuteAsync("delete LegalEntity");
                });
        }
    }

    [Table("LegalEntity")]
    internal class LegalEntity
    {
        [ExplicitKey]
        public Guid LegalEntityId { get; set; } = Guid.NewGuid();

        public string? LegalEntityName { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";
    }

    [Table("Participation")]
    internal class Participation
    {
        [ExplicitKey]
        public Guid ParticipationId { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public int? ParticipationTypeId { get; set; } = 1;

        public int? IndustryId { get; set; } = 1;

        public int? StatusId { get; set; } = 1;
    }

    [Table("Brand")]
    internal class Brand
    {
        [ExplicitKey]
        public Guid BrandId { get; set; } = Guid.NewGuid();

        public string? BrandName { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";

        public int? BrandStatusId { get; set; } = 1;

        public Guid? ParticipationId { get; set; }

        public DateTime? LastUpdated { get; set; } = DateTime.Now;
    }

    [Table("AuthDetail")]
    internal class AuthDetail
    {
        [ExplicitKey]
        public Guid BrandId { get; set; }

        public int? RegisterUTypeId { get; set; } = 1;

        public string? JwksEndpoint { get; set; } = "foo";
    }

    [Table("Endpoint")]
    internal class Endpoint
    {
        [ExplicitKey]
        public Guid BrandId { get; set; }

        public string? Version { get; set; } = "foo";

        public string? PublicBaseUri { get; set; } = "foo";

        public string? ResourceBaseUri { get; set; } = "foo";

        public string? InfosecBaseUri { get; set; } = "foo";

        public string? ExtensionBaseUri { get; set; } = "foo";

        public string? WebsiteUri { get; set; } = "foo";
    }

    [Table("SoftwareProduct")]
    internal class SoftwareProduct
    {
        [ExplicitKey]
        public Guid SoftwareProductId { get; set; } = Guid.NewGuid();

        public string? SoftwareProductName { get; set; } = "foo";

        public string? SoftwareProductDescription { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";

        public string? SectorIdentifierUri { get; set; } = "foo";

        public string? ClientUri { get; set; } = "foo";

        public string? TosUri { get; set; } = "foo";

        public string? PolicyUri { get; set; } = "foo";

        public string? RecipientBaseUri { get; set; } = "foo";

        public string? RevocationUri { get; set; } = "foo";

        public string? RedirectUris { get; set; } = "foo";

        public string? JwksUri { get; set; } = "foo";

        public string? Scope { get; set; } = "foo";

        public int? StatusId { get; set; } = 1;

        public Guid? BrandId { get; set; }
    }

    [Table("SoftwareProductCertificate")]
    internal class SoftwareProductCertificate
    {
        [ExplicitKey]
        public Guid SoftwareProductCertificateId { get; set; } = Guid.NewGuid();

        public Guid SoftwareProductId { get; set; }

        public string? CommonName { get; set; } = "foo";

        public string? Thumbprint { get; set; } = "foo";
    }

    [Table("ParticipationStatus")]
    internal class ParticipationStatus
    {
        [ExplicitKey]
        public int? ParticipationStatusId { get; set; }

        public string? ParticipationStatusCode { get; set; }
    }

    internal static class DatabaseSeeder
    {
        private static async Task Purge(SqlConnection connection)
        {
            static async Task PurgeTable(SqlConnection connection, string tableName, bool temporal = true)
            {
                // Delete data
                await connection.ExecuteAsync($"delete {tableName}");

                // Now cleanup temporal data too
                if (temporal)
                {
                    await connection.ExecuteAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
                    await connection.ExecuteAsync($"DELETE FROM [{tableName}History]");
                    await connection.ExecuteAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName}History], DATA_CONSISTENCY_CHECK = OFF))");
                }
            }

            await PurgeTable(connection, "SoftwareProductCertificate");
            await PurgeTable(connection, "SoftwareProduct");
            await PurgeTable(connection, "Endpoint");
            await PurgeTable(connection, "AuthDetail");
            await PurgeTable(connection, "Brand");
            await PurgeTable(connection, "Participation");
            await PurgeTable(connection, "LegalEntity");
        }

        public static async Task Execute()
        {
            using var connection = new SqlConnection(BaseTest.CONNECTIONSTRING_REGISTER_RW);
            await connection.OpenAsync();

            await Purge(connection);
            await TestFixture.Seeddata();

            await connection.ExecuteAsync("delete ParticipationStatus where ParticipationStatusId = -999");
            await connection.InsertAsync(new ParticipationStatus { ParticipationStatusId = -999, ParticipationStatusCode = "foo" });
        }
    }
}
