using System.Linq;
using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    public class SetupApiVersionParamsOperationFilter : IOperationFilter
    {
        private readonly CdrApiOptions _options;

        public SetupApiVersionParamsOperationFilter(IOptions<CdrApiOptions> options)
        {
            this._options = options.Value;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionOption = this._options.GetApiEndpointVersionOption($"/{context.ApiDescription.RelativePath}");

            foreach (var s in operation.Parameters.Where(o => o.Name == Headers.X_V || o.Name == Headers.X_MIN_V))
            {
                s.Required = false;

                var apiVersion = context.ApiDescription.GetApiVersion();

                switch (s.Name)
                {
                    case Headers.X_V:
                        if (versionOption?.IsXVHeaderMandatory == true)
                        {
                            s.Required = true;
                        }

                        if (apiVersion != null)
                        {
                            s.Example = new OpenApiString(apiVersion.MajorVersion.ToString());
                        }

                        break;

                    case Headers.X_MIN_V:
                        if (versionOption != null)
                        {
                            s.Example = new OpenApiString(versionOption.CurrentMinVersion.ToString());
                        }

                        break;
                }
            }
        }
    }
}
