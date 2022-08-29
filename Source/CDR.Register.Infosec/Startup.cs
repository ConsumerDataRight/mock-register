using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Middleware;
using CDR.Register.Domain.Repositories;
using CDR.Register.Infosec.Interfaces;
using CDR.Register.Infosec.Services;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Serilog;

namespace CDR.Register.Infosec
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = ModelStateErrorMiddleware.ExecuteResult;
                })
                .AddNewtonsoftJson(options => 
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter()); 
                });

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Register_DB")));
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddScoped<LogActionEntryAttribute>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRegisterInfosecRepository, RegisterInfosecRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context => await ApiExceptionHandler.Handle(context));
            });

            app.UseSerilogRequestLogging();

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
