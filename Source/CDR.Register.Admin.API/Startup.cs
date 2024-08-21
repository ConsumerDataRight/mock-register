using CDR.Register.Admin.API.Business.Validators;
using CDR.Register.Admin.API.Extensions;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Domain.Extensions;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CDR.Register.API.Infrastructure.Constants;
using Constants = CDR.Register.Admin.API.Common.Constants;
using System;
using System.Net.Mime;

namespace CDR.Register.Admin.API
{
    public partial class Startup
    {
        private bool healthCheckMigration = false;
        private string healthCheckMigrationMessage = string.Empty;
        private bool healthCheckSeedData = false;
        private string healthCheckSeedDataMessage = string.Empty;

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

            services.AddRegisterAdmin(Configuration);
            services.AddRegisterAdminAuth(Configuration);

            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new CdrVersionReader(new CdrApiOptions()); //uses default options atm
                options.ErrorResponses = new ApiVersionErrorResponse();
                options.ReportApiVersions = true;
            });

            var enableSwagger = Configuration.GetValue<bool>(ConfigurationKeys.EnableSwagger);
            if (enableSwagger)
            {
                var issuer = Configuration.GetValue<string>(Constants.Authorization.Issuer);
                services.AddCdrSwaggerGen(opt =>
                {
                    opt.SwaggerTitle = "Consumer Data Right (CDR) Participant Tooling - Mock Register - Admin API";
                    opt.IncludeAuthentication = !string.IsNullOrEmpty(issuer); //authentication is included for CTS when the issuer is not empty
                });
            }

            services.AddMvc().AddCdrNewtonsoftJson();

            services.AddValidatorsFromAssemblyContaining<BrandValidator>();
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

            var enableSwagger = Configuration.GetValue<bool>(ConfigurationKeys.EnableSwagger);
            if (enableSwagger)
            {
                app.UseCdrSwagger();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Migrate the database to the latest version during application startup.
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();

            if (serviceScope == null)
            {
                logger.LogError("ServiceScope cannot be created");
                throw new InvalidOperationException("Service scope could not be created.");
            }
            // Run EF database migrations.
            if (RunMigrations())
            {
                healthCheckMigrationMessage = "Migration in progress";
                var context = serviceScope.ServiceProvider.GetRequiredService<RegisterDatabaseContext>();
                if (context == null)
                {
                    logger.LogError("Mirgation failed. Unable to get {Name}", nameof(RegisterDatabaseContext));
                    throw new InvalidOperationException($"Unable to get {nameof(RegisterDatabaseContext)}");
                }
                context?.Database.Migrate();
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
            context.Response.ContentType = MediaTypeNames.Application.Json;
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
