using System;
using Microsoft.AspNetCore.Authorization;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public ScopeRequirement(string? scope)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public string Scope { get; }
    }
}
