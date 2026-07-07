namespace CDR.Register.API.Infrastructure.Configuration;

/// <summary>
/// The names of environment variables / configuration keys that configure the underlying Telemetry Client used by the <see cref="Serilog.Sinks.ApplicationInsights.ApplicationInsightsSink">ApplicationInsightsSink</see>.
/// </summary>
public static class ApplicationInsightsKeys
{
    /// <summary>
    /// The Connection string for Application Insights.
    /// </summary>
    public const string ConnectionString = "APPLICATIONINSIGHTS_CONNECTION_STRING";
}
