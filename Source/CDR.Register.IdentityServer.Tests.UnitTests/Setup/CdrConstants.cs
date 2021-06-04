namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public class CdrConstants
    {
        public static class CustomHeaders
        {
            public const string ClientCertThumbprintHeaderKey = "X-TlsClientCertThumbprint";

            public const string ClientCertClientNameHeaderKey = "X-TlsClientCertCN";

            public const string ContentType = "Content-Type";
        }

        public static class TokenRequest
        {
            public const string GrantType = "grant_type";

            public const string ClientId = "client_id";

            public const string ClientAssertionType = "client_assertion_type";

            public const string ClientAssertion = "client_assertion";

            public const string Scope = "scope";
        }

        public static class ClientAssertionTypes
        {
            public const string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        }

        public static class Algorithms
        {
            public const string None = "none";

            public static class Signing
            {
                public const string ES256 = "ES256";
                public const string PS256 = "PS256";
            }

            public static class Jwe
            {
                public static class Alg
                {
                    public const string RSAOAEP = "RSA-OAEP";
                    public const string RSAOAEP256 = "RSA-OAEP-256";
                    public const string RSA15 = "RSA1_5";
                }

                public static class Enc
                {
                    public const string A128GCM = "A128GCM";
                    public const string A192GCM = "A192GCM";
                    public const string A256GCM = "A256GCM";
                    public const string A128CBCHS256 = "A128CBC-HS256";
                    public const string A192CBCHS384 = "A192CBC-HS384";
                    public const string A256CBCHS512 = "A256CBC-HS512";
                }
            }
        }

    }
}
