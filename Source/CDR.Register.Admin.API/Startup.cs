using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using CDR.Register.Admin.API.Business.Validators;
using CDR.Register.Repository;
using CDR.Register.Admin.API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Admin.API.Business;

namespace CDR.Register.Admin.API
{
    public class Startup
    {
        static private bool healthCheckMigration = false;
        static private string healthCheckMigrationMessage = null;
        static private bool healthCheckSeedData = false;
        static private string healthCheckSeedDataMessage = null;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("migration", () => healthCheckMigration ? HealthCheckResult.Healthy(healthCheckMigrationMessage) : HealthCheckResult.Unhealthy(healthCheckMigrationMessage))
                .AddCheck("seed-data", () => healthCheckSeedData ? HealthCheckResult.Healthy(healthCheckSeedDataMessage) : HealthCheckResult.Unhealthy(healthCheckSeedDataMessage));

            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ApiVersionReader = new HeaderApiVersionReader(Register.API.Infrastructure.Constants.Headers.X_V);
                options.ReportApiVersions = true;
                options.ErrorResponses = new ErrorResponseVersion();
            });

            services.AddValidatorsFromAssemblyContaining<BrandValidator>();
            
            var issuer = Configuration.GetValue<string>(Constants.Authorization.Issuer);
            if (!string.IsNullOrEmpty(issuer))
            {
                // ***Injecting Jwt bearer token middleware for CTS Authentication***
                var clientId = Configuration.GetValue<string>(Constants.Authorization.ClientId);

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            // Sets the authority to Azure AD.                                                         
                            options.Authority = issuer;
                            options.TokenValidationParameters = new TokenValidationParameters()
                            {
                                RequireSignedTokens = true,

                                // Validates the issuer to be Azure AD.
                                ValidateIssuer = true,                                
                                ValidIssuer = issuer,

                                // Validates that the intended audience for the access token is the API app.
                                ValidateAudience = true,                                
                                // the application id of the API App Registration.
                                ValidAudience = clientId, 

                                ValidateLifetime = true,
                            };
                        });
            }

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Register_DBO")));

            services.AddScoped<LogActionEntryAttribute>();
            services.AddScoped<IRegisterAdminRepository, RegisterAdminRepository>();
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddAutoMapper(typeof(Startup), typeof(RegisterDatabaseContext));
            services.AddTransient<SoftwareScopeResolver>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = CustomResponseWriter
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            var issuer = Configuration.GetValue<string>(Constants.Authorization.Issuer);
            if (!string.IsNullOrEmpty(issuer))
            {
                app.UseAuthentication();
            }
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Migrate the database to the latest version during application startup.
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // Run EF database migrations.
                if (RunMigrations())
                {
                    healthCheckMigrationMessage = "Migration in progress";
                    var context = serviceScope.ServiceProvider.GetRequiredService<RegisterDatabaseContext>();
                    context.Database.Migrate();
                    healthCheckMigrationMessage = "Migration completed";

                    // Seed the database using the sample data JSON.
                    var seedDataFilePath = Configuration.GetValue<string>("SeedData:FilePath");
                    var seedDataOverwrite = Configuration.GetValue<bool>("SeedData:OverwriteExistingData", false);
                    if (!string.IsNullOrEmpty(seedDataFilePath))
                    {
                        healthCheckSeedDataMessage = "Seeding of data in progress";
                        logger.LogInformation("Seed data file found within configuration.  Attempting to seed the repository from the seed data...");
                        Task.Run(() => context.SeedDatabaseFromJsonFile(seedDataFilePath, logger, seedDataOverwrite)).Wait();
                        healthCheckSeedDataMessage = "Seeding of data completed";
                    }
                }

                // If we get here migration (if required) and seeding (if required) has completed
                healthCheckMigration = true;
                healthCheckSeedData = true;
            }
        }

        /// <summary>
        /// Determine if EF Migrations should run.
        /// </summary>
        private bool RunMigrations()
        {
            // Run migrations if the DBO connection string is set.
            var dbo = Configuration.GetConnectionString("Register_DBO");
            return !string.IsNullOrEmpty(dbo);
        }

        private static Task CustomResponseWriter(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(new
            {
                status = healthReport.Entries.Select(e => new
                {
                    key = e.Key,
                    value = e.Value.Status.ToString(),
                })
            });
            return context.Response.WriteAsync(result);
        }
    }
}
