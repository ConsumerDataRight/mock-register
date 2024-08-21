namespace CDR.Register.API.Infrastructure
{
    public static class Constants
    {
        public static class Headers
        {
            public const string X_V = "x-v";
            public const string X_MIN_V = "x-min-v";
            public const string X_TLS_CLIENT_CERT_THUMBPRINT = "X-TlsClientCertThumbprint";
            public const string X_TLS_CLIENT_CERT_COMMON_NAME = "X-TlsClientCertCN";
        }

        public static class ConfigurationKeys
        {
            public const string BasePath = "BasePath";
            public const string BasePathExpression = "BasePathExpression";            
            public const string PublicHostName = "PublicHostName";
            public const string SecureHostName = "SecureHostName";
            public const string OidcMetadataAddress = "OidcMetadataAddress";
            public const string ClockSkewSeconds = "ClockSkewSeconds";
            public const string CertThumbprintNameHttpHeaderName = "Certificate:CertThumbprintNameHttpHeaderName";
            public const string CertCommonNameHttpHeaderName = "Certificate:CertCommonNameHttpHeaderName";
            public const string EnableSwagger = "EnableSwagger";
            public const string IsServerCertificateValidationEnabled = "EnableServerCertificateValidation";
        }

        public static class Versioning
        {
            public const string GroupNameFormat = "'v'VVV";
        }

    }
}
