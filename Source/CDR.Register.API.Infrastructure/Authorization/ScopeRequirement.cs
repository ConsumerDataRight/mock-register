using System;
using Microsoft.AspNetCore.Authorization;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public string Scope { get; }

        public ScopeRequirement(string? scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }
    }    
}