using CDR.Register.API.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.IO;

namespace CDR.Register.Admin.API
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .Build();
            ConfigureSerilog(configuration);

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args, configuration).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Configure Serilog logging.
        /// </summary>
        /// <param name="configuration">App configuration.</param>
        /// <param name="isDatabaseReady">Set to True if the database is ready and the MSSqlServer sink will be configured.</param>
        public static void ConfigureSerilog(IConfiguration configuration, bool isDatabaseReady = false)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            // If the database is ready, configure the SQL Server sink
            if (isDatabaseReady)
            {
                loggerConfiguration.ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = "SerilogMSSqlServerWriteTo" });
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseRegister(configuration);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
