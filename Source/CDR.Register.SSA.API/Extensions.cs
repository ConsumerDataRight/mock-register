using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.SSA.API.Business;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace CDR.Register.SSA.API
{
    public static class Extensions
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

            services.AddMediatR(typeof(Startup));

            // Authentication
            services.AddAuthenticationAuthorization(configuration);

            return services;
        }

        public static IServiceCollection AddRegisterSSASwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CDR Register SSA API", Version = "v1" });
            });

            services.AddSwaggerGenNewtonsoftSupport();
            services.AddMvc().AddNewtonsoftJson(options => { options.SerializerSettings.Converters.Add(new StringEnumConverter()); });

            return services;
        }

        public static IApplicationBuilder UseRegisterSSASwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDR Register SSA API v1"));

            return app;
        }
    }
}
