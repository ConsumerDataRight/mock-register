using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Middleware;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using CDR.Register.API.Logger;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CDR.Register.Status.API
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
            services.AddRegisterStatus(Configuration)
                    .AddRegisterStatusSwagger();

            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ErrorResponses = new ErrorResponseVersion();
                options.ApiVersionSelector = new ApiVersionSelector(options);
            });

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Register_DB")));

            services.AddAutoMapper(typeof(Startup), typeof(RegisterDatabaseContext));

            services.AddScoped<LogActionEntryAttribute>();

            if (Configuration.GetSection("SerilogRequestResponseLogger") != null)
            {
                Log.Logger.Information("Adding request response logging middleware");
                services.AddRequestResponseLogging();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context => await ApiExceptionHandler.Handle(context));
            });

            app.UseSerilogRequestLogging();

            app.UseRegisterStatusSwagger();

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
