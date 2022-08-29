using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
