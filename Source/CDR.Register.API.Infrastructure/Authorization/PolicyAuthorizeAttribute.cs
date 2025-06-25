using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PolicyAuthorizeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public PolicyAuthorizeAttribute(RegisterAuthorisationPolicy policy)
        {
            this.RegisterAuthorisationPolicy = policy;
        }

        public RegisterAuthorisationPolicy RegisterAuthorisationPolicy { get; private set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authorizationService = (IAuthorizationService?)context.HttpContext.RequestServices.GetService(typeof(IAuthorizationService));

            if (authorizationService == null)
            {
                return;
            }

            var authorizationResult = await authorizationService.AuthorizeAsync(context.HttpContext.User, this.RegisterAuthorisationPolicy.ToString());

            if (authorizationResult.Succeeded)
            {
                return;
            }

            if (authorizationResult.Failure.FailedRequirements.Any(r => r.GetType() == typeof(MtlsRequirement)))
            {
                context.Result = new RegisterUnauthorizedResult(new ResponseErrorList(StatusCodes.Status401Unauthorized.ToString(), HttpStatusCode.Unauthorized.ToString(), "invalid_token"));
                return;
            }

            context.Result = new RegisterForbidResult(new ResponseErrorList(StatusCodes.Status403Forbidden.ToString(), HttpStatusCode.Forbidden.ToString(), "You do not have permission to access this resource."));
        }
    }
}
