using CDR.Register.Repository;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Status.API.Business;
using CDR.Register.Status.API.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace CDR.Register.Status.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterStatus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegisterStatusRepository, RegisterStatusRepository>();
            services.AddScoped<IStatusService, StatusService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StatusController).Assembly));

            return services;
        }
    }
}
