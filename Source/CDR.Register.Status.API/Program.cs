using System;
using System.IO;
using CDR.Register.API.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CDR.Register.Status.API
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

            Log.Logger = new LoggerConfiguration().ConfigureSerilog(configuration, null).CreateBootstrapLogger();

            Serilog.Debugging.SelfLog.Enable(msg => Log.Logger.Debug(msg));

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
                .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration.ConfigureSerilog(context.Configuration, services))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseRegister(configuration);
                    webBuilder.UseStartup<Startup>();
                });
    }
}
