using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Settings.Configuration;

namespace CDR.Register.API.Logger
{
    public class RequestResponseLogger : IRequestResponseLogger, IDisposable
    {
        private readonly Serilog.Core.Logger _logger;

        public RequestResponseLogger(IConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration();

            // If the Serilog response logging is disabled, do not configure it using the appsettings.
            var isSerilogRequestResponseLoggerDisabled = !configuration.GetValue<bool>("SerilogRequestResponseLogger:Enabled", true);
            if (isSerilogRequestResponseLoggerDisabled)
            {
                Debug.WriteLine("Request/Response logging is disabled");
                this._logger = loggerConfiguration.CreateLogger();
                return;
            }

            Debug.WriteLine("Request/Response logging is enabled");
            var options = new ConfigurationReaderOptions { SectionName = "SerilogRequestResponseLogger" };

            this._logger = loggerConfiguration
                .ReadFrom.Configuration(configuration, options)
                .Enrich.WithProperty("RequestMethod", string.Empty)
                .Enrich.WithProperty("RequestBody", string.Empty)
                .Enrich.WithProperty("RequestHeaders", string.Empty)
                .Enrich.WithProperty("RequestPath", string.Empty)
                .Enrich.WithProperty("RequestQueryString", string.Empty)
                .Enrich.WithProperty("StatusCode", string.Empty)
                .Enrich.WithProperty("ElapsedTime", string.Empty)
                .Enrich.WithProperty("ResponseHeaders", string.Empty)
                .Enrich.WithProperty("ResponseBody", string.Empty)
                .Enrich.WithProperty("RequestHost", string.Empty)
                .Enrich.WithProperty("RequestIpAddress", string.Empty)
                .Enrich.WithProperty("ClientId", string.Empty)
                .Enrich.WithProperty("SoftwareId", string.Empty)
                .Enrich.WithProperty("DataHolderBrandId", string.Empty)
                .Enrich.WithProperty("FapiInteractionId", string.Empty)
                .CreateLogger();
        }

        public ILogger Log
        {
            get { return this._logger; }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Serilog.Log.CloseAndFlush();
            }
        }
    }
}
