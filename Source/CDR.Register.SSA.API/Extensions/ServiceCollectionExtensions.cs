using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.SSA.API.Business;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CDR.Register.SSA.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterSSA(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISoftwareStatementAssertionRepository, SoftwareStatementAssertionRepository>();
            services.AddScoped<ISSAService, SSAService>();
            services.AddScoped<IDataRecipientStatusCheckService, DataRecipientStatusCheckService>();
            services.AddScoped<IRegisterDiscoveryRepository, RegisterDiscoveryRepository>();
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddSingleton<IMapper, Mapper>();
            services.AddSingleton<ICertificateService, CertificateService>();
            services.AddSingleton<ITokenizerService, TokenizerService>();

            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("Register_DB")));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            // Authentication
            services.AddAuthenticationAuthorization(configuration);

            return services;
        }
    }
}
