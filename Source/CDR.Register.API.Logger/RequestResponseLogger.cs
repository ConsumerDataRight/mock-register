namespace CDR.Register.API.Logger
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Core;
    using Serilog.Settings.Configuration;

    public class RequestResponseLogger : IRequestResponseLogger, IDisposable
    {
        private readonly Logger _logger;

        public ILogger Log
        {
            get { return _logger; }
        }

        public RequestResponseLogger(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
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

        public void Dispose()
        {
            Dispose(true);
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
