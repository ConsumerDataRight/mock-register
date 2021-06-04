namespace CDR.Register.IdentityServer.Configurations
{
    public static class Constants
    {
        public static class SecretTypes
        {
            public const string JwksUrl = "JWKSURL";
        }

        public static class ParsedSecretTypes
        {
            public const string CdrSecret = "CdrSecret";
        }

        public static class ConfigurationKeys
        {
            public const string IssuerUri = "IssuerUri";
            public const string JwksUri = "JwksUri";
            public const string TokenUri = "TokenUri";
        }

        public static class DiscoveryOverrideKeys
        {
            public const string JwksUri = "jwks_uri_override";
            public const string TokenEndpoint = "token_endpoint_override";
        }
    }

}
