using System;

namespace CDR.Register.API.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class AuthorisationPolicyAttribute : Attribute
    {
        public AuthorisationPolicyAttribute(string name, string? scopeRequirement, bool hasMtlsRequirement, bool hasHolderOfKeyRequirement, bool hasAccessTokenRequirement)
        {
            Name = name;
            ScopeRequirement = scopeRequirement;
            HasMtlsRequirement = hasMtlsRequirement;
            HasHolderOfKeyRequirement = hasHolderOfKeyRequirement;
            HasAccessTokenRequirement = hasAccessTokenRequirement;
        }

        public string Name { get; private set; }
        public string? ScopeRequirement { get; private set; }
        public bool HasMtlsRequirement { get; private set; }
        public bool HasHolderOfKeyRequirement { get; private set; }
        public bool HasAccessTokenRequirement { get; private set; }
    }
}
