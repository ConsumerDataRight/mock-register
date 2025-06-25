using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.Configuration
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly CdrSwaggerOptions _options;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptions<CdrSwaggerOptions> options)
        {
            this._provider = provider;
            this._options = options.Value;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in this._provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                  description.GroupName,
                  new Microsoft.OpenApi.Models.OpenApiInfo()
                  {
                      Title = this._options.SwaggerTitle,
                      Version = description.ApiVersion.ToString(),
                  });
                options.UseInlineDefinitionsForEnums();
            }
        }
    }
}
