using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Discovery.API.Business;
using CDR.Register.Repository;
using CDR.Register.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CDR.Register.Discovery.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegisterDiscoveryRepository, RegisterDiscoveryRepository>();
            services.AddScoped<IDiscoveryService, DiscoveryService>();
            services.AddScoped<IDataRecipientStatusCheckService, DataRecipientStatusCheckService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            // Add Authentication and Authorization
            services.AddAuthenticationAuthorization(configuration);

            return services;
        }
    }
}
