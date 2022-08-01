using System.Security.Cryptography.X509Certificates;

namespace CDR.Register.API.Infrastructure
{
    public interface ICertificateValidator
    {
        void ValidateClientCertificate(X509Certificate2 clientCert);
    }
}
