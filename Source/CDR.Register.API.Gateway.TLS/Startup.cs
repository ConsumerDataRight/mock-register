using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

namespace CDR.Register.API.Gateway.TLS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot();
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
                    // Send through the original host name to the backend service.
                    httpContext.Request.Headers.Append("X-Forwarded-Host", httpContext.Request.Host.ToString());

                    await next.Invoke();
                },
            };

            app.UseOcelot(pipelineConfiguration).Wait();
        }
    }
}
