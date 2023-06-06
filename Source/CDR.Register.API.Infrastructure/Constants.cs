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

        public static class ErrorCodes
        {
            public const string FieldMissing = "urn:au-cds:error:cds-all:Field/Missing";
            public const string FieldInvalid = "urn:au-cds:error:cds-all:Field/Invalid";
            public const string HeaderMissing = "urn:au-cds:error:cds-all:Header/Missing";
            public const string VersionInvalid = "urn:au-cds:error:cds-all:Header/InvalidVersion";
        }

        public static class ErrorTitles
        {
            public const string FieldMissing = "Missing Required Field";
            public const string FieldInvalid = "Invalid Field";
            public const string HeaderMissing = "Missing Required Header";
            public const string VersionInvalid = "Invalid Version";
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
        }
    }
}
