namespace CDR.Register.Infosec
{
    public static class Constants
    {
        public static class ConfigurationKeys
        {
            public const string Issuer = "Issuer";
            public const string JwksUri = "JwksUri";
            public const string TokenEndpoint = "TokenEndpoint";
        }

        public static class Scopes
        {
            public const string RegisterRead = "cdr-register:read";
            public const string RegisterBankRead = "cdr-register:bank:read";
        }

        public static class ConnectionStringNames
        {
            public const string Register = "Register_DB";
            public const string Logging = "Register_Logging_DB";
            public const string RequestResponseLogging = "Register_RequestResponse_Logging_DB";
            public const string Cache = "Register_Cache";
        }
    }
}
