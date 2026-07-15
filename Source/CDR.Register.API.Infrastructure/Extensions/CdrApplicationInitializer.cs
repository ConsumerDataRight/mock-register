using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serilog
{
    public class CdrApplicationInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var ass = Assembly.GetEntryAssembly()?.GetName();

            if (ass is null)
            {
                return;
            }

            telemetry.Context.Component.Version = ass.Version?.ToString();
            telemetry.Context.Cloud.RoleName = ass.Name;
        }
    }
}
