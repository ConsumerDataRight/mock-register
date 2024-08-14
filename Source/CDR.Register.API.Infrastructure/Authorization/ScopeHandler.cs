using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Linq;
using System.Threading.Tasks;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class ScopeHandler : AuthorizationHandler<ScopeRequirement>
    {
        private readonly ILogger<ScopeHandler> _logger;

        public ScopeHandler(ILogger<ScopeHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            // Check that authentication was successful before doing anything else
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope"))
            {
                using (LogContext.PushProperty("MethodName", "HandleRequirementAsync"))
                {
                    _logger.LogError("Unauthorized request. Access token is missing 'scope' claim.");
                }
                return Task.CompletedTask;
            }

            // Return the user claim scope
            var userClaimScopes = context.User.FindFirst(c => c.Type == "scope")?.Value.Split(' ');

            // Succeed if the scope array contains the required scope
            // The space character is used to seperate the scopes as this is in line with CDS specifications.
            string[] requiredScopes = requirement.Scope.Split(' ');

            if (userClaimScopes!=null && userClaimScopes.Intersect(requiredScopes).Any())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}