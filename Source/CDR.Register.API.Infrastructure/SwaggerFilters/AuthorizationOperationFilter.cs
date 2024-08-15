using CDR.Register.API.Infrastructure.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.API.Infrastructure.SwaggerFilters
{
    /// <summary>
    /// As this filter is used across mutiple solutions and we haven't got a common library to draw from yet, the 'latest version' has been copied across and we have kept all of the code to ensure consistency across solutions.
    /// The lines that could not be included without significant work that would currently provide no added value have been commented and intentionally left here for clear alignment.
    /// </summary>
    public class AuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var typeList = new List<Type>() { typeof(PolicyAuthorizeAttribute) };
            var relAtts = AttributeExtensions.GetAttributes(typeList, context.MethodInfo, true);
            
            var authAtt = (PolicyAuthorizeAttribute?)relAtts.FirstOrDefault(attr => attr.GetType() == typeof(PolicyAuthorizeAttribute));

            var openApiObj = new OpenApiObject();

            if (authAtt != null)
            {
                //Get the details of the policy
                var authPolicy = authAtt.policy.GetPolicy();

                if (authPolicy != null)
                {
                    AddPolicyRequirements(openApiObj, authPolicy);
                }
            }              

            operation.Extensions.Add("x-authorisation-policy", openApiObj);
        }

        private static void AddPolicyRequirements(OpenApiObject openApiObj, AuthorisationPolicyAttribute authPolicy)
        {
            if (authPolicy.HasHolderOfKeyRequirement)
            {
                openApiObj["hasHolderOfKeyRequirement"] = new OpenApiBoolean(true);
            }
            if (authPolicy.HasAccessTokenRequirement)
            {
                openApiObj["hasAccessTokenRequirement"] = new OpenApiBoolean(true);
            }
            if (authPolicy.HasMtlsRequirement)
            {
                openApiObj["hasMtlsRequirement"] = new OpenApiBoolean(true);
            }
            if (!authPolicy.ScopeRequirement.IsNullOrEmpty())
            {
                openApiObj["scopeRequirement"] = new OpenApiString(authPolicy.ScopeRequirement);
            }
        }
    }
}
