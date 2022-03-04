using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Threading.Tasks;

namespace CDR.Register.Admin.API
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
            services.AddControllers();

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Register_DBO")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Migrate the database to the latest version during application startup.
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {               
                const string HEALTHCHECK_READY_FILENAME = "_healthcheck_ready"; // TODO - MJS - Should be using ASPNet health check, not a file
                File.Delete(HEALTHCHECK_READY_FILENAME);

                // Run EF database migrations.
                if (RunMigrations())
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<RegisterDatabaseContext>();
                    context.Database.Migrate();

                    // Seed the database using the sample data JSON.
                    var seedDataFilePath = Configuration.GetValue<string>("SeedData:FilePath");
                    var seedDataOverwrite = Configuration.GetValue<bool>("SeedData:OverwriteExistingData", false);

                    if (!string.IsNullOrEmpty(seedDataFilePath))
                    {
                        logger.LogInformation("Seed data file found within configuration.  Attempting to seed the repository from the seed data...");
                        Task.Run(() => context.SeedDatabaseFromJsonFile(seedDataFilePath, logger, seedDataOverwrite)).Wait();
                    }
                }
                
                File.WriteAllText(HEALTHCHECK_READY_FILENAME, "");  // Create file to indicate Register is ready, this can be used by Docker/Dockercompose health checks // TODO - MJS - Should be using ASPNet health check, not a file
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
    }
}
