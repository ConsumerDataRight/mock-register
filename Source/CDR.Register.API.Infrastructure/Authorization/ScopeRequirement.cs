using System;
using Microsoft.AspNetCore.Authorization;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Scope { get; }

        public ScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }

    public class AuthFailure
    {
        public string Issuer { get; set; }
        public string Scope { get; set; }
    }
}