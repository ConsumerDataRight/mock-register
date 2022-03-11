using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

namespace CDR.Register.API.Gateway.mTLS
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICertificateValidator, CertificateValidator>();
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
             .AddCertificate(options =>
             {
                 // Basic client certificate checks.
                 options.AllowedCertificateTypes = CertificateTypes.All;
                 options.ValidateCertificateUse = true;
                 options.ValidateValidityPeriod = true;
                 options.RevocationMode = X509RevocationMode.NoCheck;

                 options.Events = new CertificateAuthenticationEvents
                 {
                     OnCertificateValidated = context =>
                     {
                         var logger = context.HttpContext.RequestServices.GetService<ILogger<Startup>>();
                         logger.LogInformation("OnCertificateValidated...");

                         var certValidator = context.HttpContext.RequestServices.GetService<ICertificateValidator>();
                         if (!certValidator.IsValid(context.ClientCertificate))
                         {
                             const string failValidationMsg = "The client certificate failed to validate";
                             logger.LogWarning(failValidationMsg);
                             context.Fail(failValidationMsg);
                         }

                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = context =>
                     {
                         context.Fail("invalid cert");
                         return Task.CompletedTask;
                     }
                 };
             })
             // Adding an ICertificateValidationCache results in certificate auth caching the results.
             // The default implementation uses a memory cache.
             .AddCertificateCache();
            services.AddOcelot();
            services.AddScoped<LogActionEntryAttribute>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            var pipelineConfiguration = new OcelotPipelineConfiguration
            {
                PreErrorResponderMiddleware = async (httpContext, next) =>
                {
                    var clientCert = await httpContext.Connection.GetClientCertificateAsync();

                    // The thumbprint and common name from the client certificate are extracted and added as headers for the downstream services.
                    if (clientCert != null)
                    {
                        httpContext.Request.Headers.Add("X-TlsClientCertThumbprint", clientCert.Thumbprint);
                        httpContext.Request.Headers.Add("X-TlsClientCertCN", clientCert.GetNameInfo(X509NameType.SimpleName, false));
                    }

                    // Send through the original host name to the backend service.
                    httpContext.Request.Headers.Add("X-Forwarded-Host", httpContext.Request.Host.ToString());

                    await next.Invoke();
                }
            };

            app.UseAuthentication();
            app.UseOcelot(pipelineConfiguration).Wait();
        }
    }
}
