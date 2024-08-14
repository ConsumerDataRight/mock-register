using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace CDR.Register.API.Infrastructure
{
    public static class CdrSwaggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCdrSwagger(this IApplicationBuilder builder, IApiVersionDescriptionProvider? provider = null)
        {
            if (provider == null)
            {
                provider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            }

            builder.UseSwagger();

            builder.UseSwaggerUI(
                options =>
                {
                    // Configure swagger Ui for multiple versions of the API
                    string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";

                    foreach (var groupName in provider.ApiVersionDescriptions.Select(d => d.GroupName))
                    {
                        options.SwaggerEndpoint(
                            $"{swaggerJsonBasePath}/swagger/{groupName}/swagger.json",
                            groupName.ToUpperInvariant());
                    }
                });

            return builder;
        }

        public static IApplicationBuilder UseCdrSwagger(this IApplicationBuilder builder, string name)
        {
            builder.UseSwagger();

            builder.UseSwaggerUI(
                options =>
                {
                    // Configure swagger Ui for multiple versions of the API
                    string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";

                    options.SwaggerEndpoint(
                             $"{swaggerJsonBasePath}/swagger/v1/swagger.json",
                            name);
                });

            return builder;
        }
    }
}
