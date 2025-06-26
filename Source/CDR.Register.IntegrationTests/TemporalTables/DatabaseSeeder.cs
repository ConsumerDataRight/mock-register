// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    internal static class DatabaseSeeder
    {
        public static async Task Execute()
        {
            using var connection = new SqlConnection(BaseTest.CONNECTIONSTRING_REGISTER_RW);
            await connection.OpenAsync();

            await Purge(connection);
            await TestFixture.Seeddata();

            await connection.ExecuteAsync("delete ParticipationStatus where ParticipationStatusId = -999");
            await connection.InsertAsync(new ParticipationStatus { ParticipationStatusId = -999, ParticipationStatusCode = "foo" });
        }

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
    }
}
