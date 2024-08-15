using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    /// <summary>
    /// The purpose of those filters is to get the auto-generated swagger into a consistent order to aid in comparison with the json schema defined at https://consumerdatastandardsaustralia.github.io/standards/includes/swagger/cds_register.json
    /// These orders don't neccessarily match yet, but we have the aim to better match them in the future and until they do, it's helpful to order them alphabetically to aid in searching for property manually.
    /// We are also exposing properties to the swagger auto-generation that are already set within the application, but don't show up on the autogeneration without this code.
    /// </summary>
    public class CustomDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths;
            foreach (var path in paths.Select(path => path.Value))
            {
                path.Parameters = path.Parameters.OrderBy(e => e.Name).ToList();

                foreach (var op in path.Operations.Select(op => op.Value))
                {
                    //api-version included as it is currently used in some places in PT instead of x-v
                    if (op.Parameters.Any(p => p.Name.Equals("x-v",System.StringComparison.OrdinalIgnoreCase)) || op.Parameters.Any(p => p.Name.Equals("api-version", System.StringComparison.OrdinalIgnoreCase)))
                    {
                        op.Extensions.TryGetValue("x-version", out var value);
                        if (value == null && !string.IsNullOrWhiteSpace(swaggerDoc.Info.Version))
                        {
                            op.Extensions.Add("x-version", new OpenApiString(swaggerDoc.Info.Version)); // just to get it generated in json
                        }
                    }

                    op.Parameters = op.Parameters.OrderBy(p => p.Name).ToList();
                }
            }

            swaggerDoc.Paths = paths;
        }
    }
}
