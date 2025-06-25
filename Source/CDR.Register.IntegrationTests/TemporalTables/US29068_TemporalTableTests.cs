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
        private delegate Task Mutate(SqlConnection connection);

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
                        LegalEntityId = await connection.ExecuteScalarAsync<Guid>("select top 1 legalentityid from legalentity"),
                    }),
                    "Brand" => connection.InsertAsync(new Brand()
                    {
                        ParticipationId = await connection.ExecuteScalarAsync<Guid>("select top 1 participationid from participation"),
                    }),
                    "AuthDetail" => connection.InsertAsync(new AuthDetail()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand"),
                    }),
                    "Endpoint" => connection.InsertAsync(new Endpoint()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand"),
                    }),
                    "SoftwareProduct" => connection.InsertAsync(new SoftwareProduct()
                    {
                        BrandId = await connection.ExecuteScalarAsync<Guid>("select top 1 brandid from brand"),
                    }),
                    "SoftwareProductCertificate" => connection.InsertAsync(new SoftwareProductCertificate()
                    {
                        SoftwareProductId = await connection.ExecuteScalarAsync<Guid>("select top 1 softwareProductid from softwareProduct"),
                    }),
                    _ => throw new NotSupportedException(),
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

        private static async Task Test(string tableName, Mutate mutate)
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

        private static async Task<string> GetTableJson(SqlConnection connection, string tableName, DateTime? pointInTimeUTC = null)
        {
            string orderby = tableName.ToUpper() switch
            {
                "AUTHDETAIL" => "BrandID",
                "ENDPOINT" => "BrandID",
                _ => $"{tableName}ID",
            };

            IEnumerable<dynamic>? data;

            string sql = @$"
                select * from {tableName} 
                {(pointInTimeUTC == null ? string.Empty : $"for system_time as of '{pointInTimeUTC:yyyy-MM-dd HH:mm:ss.fffffff}'")}
                order by {orderby}";

            data = await connection.QueryAsync(sql);

            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { });
        }
    }
}
