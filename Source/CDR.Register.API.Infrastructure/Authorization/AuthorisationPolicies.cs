using System.Collections.Generic;
using System;
using Microsoft.OpenApi.Extensions;
using System.Linq;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public static class AuthorisationPolicies
    {
        private static readonly Dictionary<RegisterAuthorisationPolicy, AuthorisationPolicyAttribute> _policies = InitPolicies();

        public static AuthorisationPolicyAttribute GetPolicy(this RegisterAuthorisationPolicy policy)
        {
            if (_policies.TryGetValue(policy, out var res))
            {
                return res;
            }

            throw new ArgumentOutOfRangeException($"Policy {policy} doesn't have any Authorisation Policy attribute");
        }

        public static List<AuthorisationPolicyAttribute> GetAllPolicies()
        {
            return _policies.Select(p => p.Value).ToList();
        }

        private static Dictionary<RegisterAuthorisationPolicy, AuthorisationPolicyAttribute> InitPolicies()
        {
            var result = new Dictionary<RegisterAuthorisationPolicy, AuthorisationPolicyAttribute>();
            foreach (RegisterAuthorisationPolicy policy in Enum.GetValues(typeof(RegisterAuthorisationPolicy)))
            {
                var attr = policy.GetAttributeOfType<AuthorisationPolicyAttribute>();

                if (attr != null)
                {
                    result.Add(policy, new AuthorisationPolicyAttribute(attr.Name, attr.ScopeRequirement, attr.HasMtlsRequirement, attr.HasHolderOfKeyRequirement, attr.HasAccessTokenRequirement));
                }
            }

            return result;
        }
    }
}
