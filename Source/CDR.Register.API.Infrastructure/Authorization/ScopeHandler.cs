using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

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
            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            {
                _logger.LogError($"Unauthorized request. Access token is missing 'scope' claim for issuer '{requirement.Issuer}'.");
                return Task.CompletedTask;
            }

            // Split the scopes string into an array
            var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer).Value.Split(' ');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == requirement.Scope))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogError($"Unauthorized request. Access token does not contain scope '{requirement.Scope}' for issuer '{requirement.Issuer}'.");
            }

            return Task.CompletedTask;
        }
    }
}
