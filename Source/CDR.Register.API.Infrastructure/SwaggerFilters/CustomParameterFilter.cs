using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    public class CustomParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            parameter.Extensions.TryGetValue("explode", out var value);

            if (value == null)
            {
                parameter.Extensions.Add("explode", new OpenApiBoolean(parameter.Explode)); // just to get it generated in json
            }

            parameter.Style = ParameterStyle.Simple;
        }
    }
}
