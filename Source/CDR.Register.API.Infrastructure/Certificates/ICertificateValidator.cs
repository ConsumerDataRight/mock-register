using System.Security.Cryptography.X509Certificates;

namespace CDR.Register.API.Infrastructure
{
    public interface ICertificateValidator
    {
        bool IsValid(X509Certificate2 clientCert);
    }
}
