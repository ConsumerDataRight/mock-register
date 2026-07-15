using System;
using System.IO;
using CDR.Register.API.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;

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
#if DEBUG
                .AddUserSecrets(typeof(Program).Assembly)
#endif
                .Build();

            Log.Logger = ConfigureSerilog(configuration, null, new LoggerConfiguration()).CreateBootstrapLogger();

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

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, loggerConfiguration) => ConfigureSerilog(context.Configuration, services, loggerConfiguration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseRegister(configuration);
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Configure Serilog logging.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="services">The services (optional) in order to configure the logger.</param>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        private static LoggerConfiguration ConfigureSerilog(IConfiguration configuration, IServiceProvider? services, LoggerConfiguration loggerConfiguration)
        {
            var modified = LoggingRegistrationExtensions.ConfigureSerilog(loggerConfiguration, configuration, services);

            if (services != null)
            {
                // The application has bootstrapped and the database exists, we can use it for logs.
                modified = modified.ReadFrom.Configuration(configuration, new ConfigurationReaderOptions() { SectionName = "SerilogMSSqlServerWriteTo" });
            }

            return modified;
        }
    }
}
