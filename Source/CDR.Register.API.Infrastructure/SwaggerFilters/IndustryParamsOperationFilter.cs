using System.Linq;
using System.Reflection;
using CDR.Register.Domain.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    public class IndustryParamsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var s in operation.Parameters.Where(o => o.Name == "industry"))
            {
                var checkIndustries = context.MethodInfo.GetCustomAttributes<Filters.CheckIndustryAttribute>().SingleOrDefault();

                if (checkIndustries != null)
                {
                    var examples = checkIndustries.IndustryRestrictions.Select(x => new OpenApiString(EnumExtensions.GetDescription(x)));

                    s.Schema.Enum = [..examples];
                }
            }
        }
    }
}
