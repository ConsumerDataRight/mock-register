using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDR.Register.Repository.Migrations
{
    /// <summary>
    /// Add tables for Serilog MSSQL Logging Sinks.
    /// </summary>
    public partial class AddLoggingTables : Migration
    {
        private const string CreateRequestResponseTableIfMissingSql =
                """
                IF OBJECT_ID(N'dbo.[LogEvents-RequestResponse]', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.[LogEvents-RequestResponse] (
                        [Id]                 INT IDENTITY (1,1) NOT NULL PRIMARY KEY,
                        [Message]            NVARCHAR (MAX) NULL,
                        [Level]              NVARCHAR (MAX) NULL,
                        [TimeStamp]          DATETIME NULL,
                        [Exception]          NVARCHAR (MAX) NULL,
                        [SourceContext]      VARCHAR (100) NULL,
                        [ClientId]           VARCHAR (50) NULL,
                        [SoftwareId]         VARCHAR (50) NULL,
                        [DataHolderBrandId]  VARCHAR (50) NULL,
                        [FapiInteractionId]  VARCHAR (50) NULL,
                        [RequestMethod]      VARCHAR (20) NULL,
                        [RequestBody]        VARCHAR (MAX) NULL,
                        [RequestHeaders]     VARCHAR (MAX) NULL,
                        [RequestPath]        VARCHAR (2000) NULL,
                        [RequestQueryString] VARCHAR (4000) NULL,
                        [StatusCode]         VARCHAR (20) NULL,
                        [ElapsedTime]        VARCHAR (20) NULL,
                        [RequestHost]        VARCHAR (4000) NULL,
                        [RequestIpAddress]   VARCHAR (50) NULL,
                        [ResponseHeaders]    VARCHAR (4000) NULL,
                        [ResponseBody]       VARCHAR (MAX) NULL
                    );
                END;
                """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var logEventTables = new[]
            {
                "LogEvents-Admin-API",
                "LogEvents-Discovery-API",
                "LogEvents-Gateway-mTLS",
                "LogEvents-Gateway-TLS",
                "LogEvents-Infosec",
                "LogEvents-SSA-API",
                "LogEvents-Status-API",
            };

            foreach (var tableName in logEventTables)
            {
                migrationBuilder.Sql(CreateLogEventTableIfMissingSql(tableName));
            }

            // Request / Response log table (distinct schema)
            migrationBuilder.Sql(CreateRequestResponseTableIfMissingSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op.
            // These tables may pre-exist or be shared with other systems
            // and must not be removed by EF migrations.
        }

        private static string CreateLogEventTableIfMissingSql(string tableName)
            => $"""
                IF OBJECT_ID(N'dbo.[{tableName}]', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.[{tableName}] (
                        [Id]            INT IDENTITY (1,1) NOT NULL PRIMARY KEY,
                        [Message]       NVARCHAR (MAX) NULL,
                        [Level]         NVARCHAR (MAX) NULL,
                        [TimeStamp]     DATETIME NULL,
                        [Exception]     NVARCHAR (MAX) NULL,
                        [Environment]   NVARCHAR (50) NULL,
                        [ProcessId]     NVARCHAR (50) NULL,
                        [ProcessName]   NVARCHAR (50) NULL,
                        [ThreadId]      NVARCHAR (50) NULL,
                        [MethodName]    NVARCHAR (50) NULL,
                        [SourceContext] NVARCHAR (100) NULL
                    );
                END;
                """;
    }
}
