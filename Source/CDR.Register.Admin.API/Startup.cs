using System.Threading.Tasks;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            services.AddDbContext<RegisterDatabaseContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
        }
    }
}
