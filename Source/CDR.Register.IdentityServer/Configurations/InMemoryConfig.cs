using System.Collections.Generic;
using CDR.Register.Repository.Infrastructure;
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
                        CdsRegistrationScopes.BankRead, CdsRegistrationScopes.Read
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
                return new List<ApiScope> { new ApiScope(CdsRegistrationScopes.BankRead, "CDR Register"), new ApiScope(CdsRegistrationScopes.Read, "CDR Register2") };
            }
        }
    }
}