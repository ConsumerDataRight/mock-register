using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;

namespace CDR.Register.IdentityServer.Configurations
{
    public static class InMemoryConfig
    {
        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource()
                {
                    Name = "cdr-register",
                    Description =  "Consumer Data Right (CDR) Register API",
                    UserClaims = { JwtClaimTypes.Subject },
                    Scopes =
                    {
                        "cdr-register:bank:read"
                    }
                }
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiScope> Scopes
        {
            get
            {
                return new List<ApiScope> { new ApiScope("cdr-register:bank:read", "CDR Register") };
            }
        }
    }
}
