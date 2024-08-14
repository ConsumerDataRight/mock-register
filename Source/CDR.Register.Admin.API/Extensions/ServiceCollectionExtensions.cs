using CDR.Register.Admin.API.Business;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Constants = CDR.Register.Admin.API.Common.Constants;

namespace CDR.Register.Admin.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegisterAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<LogActionEntryAttribute>();
            services.AddScoped<IRegisterAdminRepository, RegisterAdminRepository>();
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddAutoMapper(typeof(Startup), typeof(RegisterDatabaseContext));
            services.AddTransient<SoftwareScopeResolver>();

            // This is to manage the EF database context through the web API DI.
            // If this is to be done inside the repository project itself, we need to manage the context life-cycle explicitly.
            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("Register_DBO")));

            return services;
        }

        public static IServiceCollection AddRegisterAdminAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var issuer = configuration.GetValue<string>(Constants.Authorization.Issuer);
            if (!string.IsNullOrEmpty(issuer))
            {
                // ***Injecting Jwt bearer token middleware for CTS Authentication***
                var clientId = configuration.GetValue<string>(Constants.Authorization.ClientId);

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

            return services;
        }
    }
}
