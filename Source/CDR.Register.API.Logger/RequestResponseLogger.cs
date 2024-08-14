namespace CDR.Register.API.Logger
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Core;
    using Serilog.Settings.Configuration;

    public class RequestResponseLogger : IRequestResponseLogger, IDisposable
    {
        private readonly Logger _logger;

        public ILogger Log { get { return _logger; } }

        public RequestResponseLogger(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions { SectionName = "SerilogRequestResponseLogger" })
                .Enrich.WithProperty("RequestMethod", "")
                .Enrich.WithProperty("RequestBody", "")
                .Enrich.WithProperty("RequestHeaders", "")
                .Enrich.WithProperty("RequestPath", "")
                .Enrich.WithProperty("RequestQueryString", "")
                .Enrich.WithProperty("StatusCode", "")
                .Enrich.WithProperty("ElapsedTime", "")
                .Enrich.WithProperty("ResponseHeaders", "")
                .Enrich.WithProperty("ResponseBody", "")
                .Enrich.WithProperty("RequestHost", "")
                .Enrich.WithProperty("RequestIpAddress", "")
                .Enrich.WithProperty("ClientId", "")
                .Enrich.WithProperty("SoftwareId", "")
                .Enrich.WithProperty("DataHolderBrandId", "")
                .Enrich.WithProperty("FapiInteractionId", "")
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