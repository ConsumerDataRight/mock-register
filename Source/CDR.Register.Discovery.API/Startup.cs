using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Middleware;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using CDR.Register.API.Logger;
using CDR.Register.Discovery.API.Extensions;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using static CDR.Register.API.Infrastructure.Constants;
using CDR.Register.Domain.Extensions;

namespace CDR.Register.Discovery.API
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
            services.AddHttpContextAccessor();

            services.AddRegisterDiscovery(Configuration);

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = ModelStateErrorMiddleware.ExecuteResult;
                });

            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new CdrVersionReader(new CdrApiOptions()); //uses default options atm
                options.ErrorResponses = new ApiVersionErrorResponse();
            });

            var enableSwagger = Configuration.GetValue<bool>(ConfigurationKeys.EnableSwagger);
            if (enableSwagger)
            {
                services.AddCdrSwaggerGen(opt =>
                {
                    opt.SwaggerTitle = "Consumer Data Right (CDR) Participant Tooling - Mock Register - Discovery API";
                    opt.IncludeAuthentication = true;
                });
            }

            services.AddMvc().AddCdrNewtonsoftJson();

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

            app.UseBasePathOrExpression(Configuration);

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            var enableSwagger = Configuration.GetValue<bool>(ConfigurationKeys.EnableSwagger);
            if (enableSwagger)
            {
                app.UseCdrSwagger();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
