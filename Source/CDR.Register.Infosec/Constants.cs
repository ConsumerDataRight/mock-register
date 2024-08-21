namespace CDR.Register.Infosec
{
    public static class Constants
    {        
        public static class Scopes
        {
            public const string RegisterRead = "cdr-register:read";
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
