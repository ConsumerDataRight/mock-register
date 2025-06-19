using System;
using System.Linq;
using CDR.Register.Admin.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace CDR.Register.Admin.API.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = (IConfiguration?)context.HttpContext.RequestServices
                .GetService(typeof(IConfiguration));

            // Ignore if the Authentication is disabled.
            var issuer = configuration?.GetValue<string>(Constants.Authorization.Issuer);
            if (string.IsNullOrEmpty(issuer))
            {
                return;
            }

            var user = context.HttpContext.User;
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                InvalidToken(context);
            }

            // Scope, Role validation must have Admin API write access
            var scopeAttributeName = configuration?.GetValue<string>(Constants.Authorization.ScopeAttributeName);
            var scopeValue = configuration?.GetValue<string>(Constants.Authorization.ScopeValue);
            var allowedScope = !string.IsNullOrEmpty(scopeAttributeName) && user.FindAll(scopeAttributeName)
                .Any(x => string.Equals(x.Value, scopeValue, StringComparison.OrdinalIgnoreCase));
            if (!allowedScope)
            {
                InvalidToken(context);
            }

            static void InvalidToken(AuthorizationFilterContext context)
            {
                context.Result = new JsonResult(new { error = "invalid_token" }) { StatusCode = StatusCodes.Status401Unauthorized };
                context.HttpContext.Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer error=\"invalid_token\"");
            }
        }
    }
}
