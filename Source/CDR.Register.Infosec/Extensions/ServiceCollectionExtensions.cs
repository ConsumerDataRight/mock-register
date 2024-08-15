using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Domain.Repositories;
using CDR.Register.Infosec.Interfaces;
using CDR.Register.Infosec.Services;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CDR.Register.Infosec.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterInfosec(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddScoped<LogActionEntryAttribute>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRegisterInfosecRepository, RegisterInfosecRepository>();
            services.AddHttpContextAccessor();

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString(Constants.ConnectionStringNames.Register)));

            return services;
        }
    }
}
