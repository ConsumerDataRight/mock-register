using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Middleware;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CDR.Register.SSA.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRegisterSSA(Configuration)
                    .AddRegisterSSASwagger();

            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ErrorResponses = new ErrorResponseVersion();
                options.ApiVersionSelector = new ApiVersionSelector(options);
            });

            services.AddAutoMapper(typeof(Startup), typeof(RegisterDatabaseContext));

            services.AddScoped<LogActionEntryAttribute>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context => await ApiExceptionHandler.Handle(context));
            });

            app.UseSerilogRequestLogging();

            app.UseRegisterSSASwagger();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
