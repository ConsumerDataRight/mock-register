using System;
using Microsoft.Extensions.Configuration;

namespace Serilog;

public static class LoggingRegistrationExtensions
{
    /// <summary>
    /// Configure the Serilog logger.
    /// </summary>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="services">The services (optional) in order to configure the logger.</param>
    /// <returns>The logger configuration which can be used to construct a logger.</returns>
    public static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, IServiceProvider? services)
    {
        loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .AddOpenTelemetry(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

        // services should only be null when creating and configuring a logger during application bootstrapping.
        if (services != null)
        {
            // place any configuration that is dependent on DI within this block
            loggerConfiguration
                .AddApplicationInsights(services, configuration);
        }

        return loggerConfiguration;
    }
}
