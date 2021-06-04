using CDR.Register.IdentityServer.Configurations;
using IdentityServer4.ResponseHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace CDR.Register.IdentityServer
{
    public class Startup
    {
        public readonly IConfiguration _configuration;
        readonly IHostEnvironment _env;

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddRegisterIdentityServer(_configuration, _env);
            services.AddTransient<IDiscoveryResponseGenerator, RegisterDiscoveryResponseGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            // Allow sensitive data to be logged in dev environment only
            IdentityModelEventSource.ShowPII = env.IsDevelopment();
            app.UseIdentityServer();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
            });
        }

    }
}
