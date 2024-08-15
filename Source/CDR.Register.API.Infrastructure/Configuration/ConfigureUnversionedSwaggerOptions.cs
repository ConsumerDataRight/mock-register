using CDR.Register.API.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.Configuration
{
    public class ConfigureUnversionedSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private const string _defaultVersion = "v1"; // This is what we are using as a default definition title for unversioned APIs

        private readonly CdrSwaggerOptions _options;

        public ConfigureUnversionedSwaggerOptions(IOptions<CdrSwaggerOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc(
                  _defaultVersion, // This name is used to direct to the path
                  new Microsoft.OpenApi.Models.OpenApiInfo()
                  {
                      Title = _options.SwaggerTitle,
                      Version = _defaultVersion,
                  });
            options.UseInlineDefinitionsForEnums();
        }
    }
}
