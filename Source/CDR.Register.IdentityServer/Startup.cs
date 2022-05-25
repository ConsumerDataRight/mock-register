using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Services;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace CDR.Register.IdentityServer
{
    public class Startup
    {
        public readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddRegisterIdentityServer(_configuration);
            services.AddTransient<IDiscoveryResponseGenerator, RegisterDiscoveryResponseGenerator>();
            services.AddTransient<ITokenCreationService, TokenCreationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();

            // Allow sensitive data to be logged in dev environment only
            IdentityModelEventSource.ShowPII = env.IsDevelopment();
            app.UseIdentityServer();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
            });
        }
    }
}
