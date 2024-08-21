using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    public class PropertyAlphabeticalOrderFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties != null && schema.Properties.Any())
            {
                var orderedProps = schema.Properties.OrderBy(arg => arg.Key).ToDictionary(arg => arg.Key, arg => arg.Value);
                schema.Properties = orderedProps;
            }
        }
    }
}
