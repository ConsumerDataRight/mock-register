namespace CDR.Register.API.Infrastructure.Configuration;

/// <summary>
/// The names of environment variables / configuration keys that configure the underlying OTLP Exporter used by the <see cref="Serilog.Sinks.OpenTelemetry.OpenTelemetrySink">OpenTelemetrySink</see>.
/// </summary>
/// <remarks>Refer to <see href="https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/">OTLP Exporter Configuration</see> OpenTelemetry documentation.</remarks>
public static class OpenTelemetryKeys
{
    /// <summary>
    /// A base endpoint URL for any signal type, with an optionally-specified port number.
    /// Helpful for when you’re sending more than one signal to the same endpoint and want one environment variable to control the endpoint.
    /// </summary>
    /// <remarks>
    /// Refer to <seealso href="https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/#otel_exporter_otlp_endpoint">OTEL_EXPORTER_OTLP_ENDPOINT</seealso> OpenTelemetry documentation.
    /// </remarks>
    public const string Endpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

    /// <summary>
    /// Endpoint URL for trace data only, with an optionally-specified port number.
    /// Typically ends with <c>v1/traces</c> when using OTLP/HTTP.
    /// </summary>
    /// <remarks>
    /// Refer to <seealso href="https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/#otel_exporter_otlp_traces_endpoint">OTEL_EXPORTER_OTLP_TRACES_ENDPOINT</seealso> OpenTelemetry documentation.
    /// </remarks>
    public const string TracesEndpoint = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT";

    /// <summary>
    /// Endpoint URL for metric data only, with an optionally-specified port number.
    /// Typically ends with <c>v1/metrics</c> when using OTLP/HTTP.
    /// </summary>
    /// <remarks>
    /// Refer to <seealso href="https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/#otel_exporter_otlp_metrics_endpoint">OTEL_EXPORTER_OTLP_METRICS_ENDPOINT</seealso> OpenTelemetry documentation.
    /// </remarks>
    public const string MetricsEndpoint = "OTEL_EXPORTER_OTLP_METRICS_ENDPOINT";

    /// <summary>
    /// Endpoint URL for log data only, with an optionally-specified port number.
    /// Typically ends with <c>v1/logs</c> when using OTLP/HTTP.
    /// </summary>
    /// <remarks>
    /// Refer to <seealso href="https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/#otel_exporter_otlp_logs_endpoint">OTEL_EXPORTER_OTLP_LOGS_ENDPOINT</seealso> OpenTelemetry documentation.
    /// </remarks>
    public const string LogsEndpoint = "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT";
}
