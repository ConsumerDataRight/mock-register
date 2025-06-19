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
            this._logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions { SectionName = "SerilogRequestResponseLogger" })
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
