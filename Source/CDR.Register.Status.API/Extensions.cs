using CDR.Register.Repository;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Status.API.Business;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace CDR.Register.Status.API
{
    public static class Extensions
    {
        public static IServiceCollection AddRegisterStatus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegisterStatusRepository, RegisterStatusRepository>();
            services.AddScoped<IStatusService, StatusService>();

            services.AddMediatR(typeof(Startup));

            return services;
        }

        public static IServiceCollection AddRegisterStatusSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CDR Register Status API", Version = "v1" });
            });

            services.AddSwaggerGenNewtonsoftSupport();
            services.AddMvc().AddNewtonsoftJson(options => { options.SerializerSettings.Converters.Add(new StringEnumConverter()); });

            return services;
        }

        public static IApplicationBuilder UseRegisterStatusSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDR Register Status API v1"));

            return app;
        }
    }
}
