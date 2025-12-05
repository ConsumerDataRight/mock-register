using System.Collections.Generic;
using System.Reflection;
using CDR.Register.API.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;

namespace Serilog
{
    /// <summary>
    /// Extension functionality for configuration OpenTelemetry.
    /// </summary>
    public static class OpenTelemetryConfigurationExtensions
    {
        /// <summary>
        /// Conditionally enable Open Telemetry if any of the following OpenTelemetry endpoint configuration values:
        /// <list type="bullet">
        /// <item><see cref="OpenTelemetryKeys.Endpoint">OTEL_EXPORTER_OTLP_ENDPOINT</see></item>
        /// <item><see cref="OpenTelemetryKeys.TracesEndpoint">OTEL_EXPORTER_OTLP_TRACES_ENDPOINT</see></item>
        /// <item><see cref="OpenTelemetryKeys.MetricsEndpoint">OTEL_EXPORTER_OTLP_METRICS_ENDPOINT</see></item>
        /// <item><see cref="OpenTelemetryKeys.LogsEndpoint">OTEL_EXPORTER_OTLP_LOGS_ENDPOINT</see></item>
        /// </list>
        /// have been set in line with the <see href="https://opentelemetry.io/docs/specs/otel/protocol/exporter/#configuration-options">Exporter configuration guidance</see>.
        /// </summary>
        /// <param name="loggerConfiguration">The existing logger configuration to which OpenTelemetry needs to be added.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The logger configuration with OpenTelemetry sink configured (if applicable).</returns>
        public static LoggerConfiguration AddOpenTelemetry(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            if (configuration.GetValue<string>(OpenTelemetryKeys.Endpoint) is not null
                || configuration.GetValue<string>(OpenTelemetryKeys.TracesEndpoint) is not null
                || configuration.GetValue<string>(OpenTelemetryKeys.MetricsEndpoint) is not null
                || configuration.GetValue<string>(OpenTelemetryKeys.LogsEndpoint) is not null)
            {
                loggerConfiguration.WriteTo.OpenTelemetry(configure: static x =>
                {
                    x.ResourceAttributes = new Dictionary<string, object>
                    {
                        { "resource.name", Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty },
                    };
                });
            }

            return loggerConfiguration;
        }
    }
}
