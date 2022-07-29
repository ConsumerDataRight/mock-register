using System;

namespace CDR.Register.API.Infrastructure.Exceptions
{
    public class ClientCertificateException : Exception
    {
        public ClientCertificateException(string message) : base($"An error occurred validating the client certificate: {message}")
        {
        }

        public ClientCertificateException(string message, Exception ex) : base($"An error occurred validating the client certificate: {message}", ex)
        {
        }
    }
}
