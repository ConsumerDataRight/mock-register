using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CDR.Register.API.Infrastructure
{
    public static class HttpClientHandlerExtensions
    {
        public static void SetServerCertificateValidation(this HttpClientHandler httpClientHandler, IConfiguration configuration)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback(configuration);
        }

        public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback(
            IConfiguration configuration)
        {
            bool isServerCertificateValidationEnabled = configuration.GetValue<bool>(Constants.ConfigurationKeys.IsServerCertificateValidationEnabled);
            return ServerCertificateCustomValidationCallback(isServerCertificateValidationEnabled);
        }

        public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> ServerCertificateCustomValidationCallback(
            bool isServerCertificateValidationEnabled)
        {
            return (message, serverCert, chain, errors) =>
            {
                if (!isServerCertificateValidationEnabled)
                {
                    return true;
                }

                return errors == SslPolicyErrors.None;
            };
        }
    }
}