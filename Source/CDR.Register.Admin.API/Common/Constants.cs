using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.Admin.API.Common
{
    public static class Constants
    {
        public static class Authorization
        {
            public const string Issuer = "Authorization:Issuer";
            public const string ClientId = "Authorization:ClientId";
            public const string ScopeAttributeName = "Authorization:TokenScopeAttribute";
            public const string ScopeValue = "Authorization:Scope";
        }
    }
}
