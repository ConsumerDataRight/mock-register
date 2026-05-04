using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.SSA.API.Business
{
    /// <summary>
    /// The class extracts information from the certificate.
    /// </summary>
    public class CertificateService : ICertificateService
    {
        public CertificateService(IConfiguration config)
        {
            // Create the certificate
            var cert = new X509Certificate2(config["SigningCertificate:Path"], config["SigningCertificate:Password"], X509KeyStorageFlags.Exportable);

            // Get credentials from certificate
            this.SecurityKey = new X509SecurityKey(cert);
            this.SigningCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);

            // Get certificate kid
            this.Kid = this.SigningCredentials.Kid;

            this.SigningCredentials.CryptoProviderFactory = new CryptoProviderFactory();

            // Get signature provider
            this.SignatureProvider = this.SigningCredentials.CryptoProviderFactory.CreateForSigning(this.SecurityKey, "PS256");

            this.JsonWebKeySet = this.GenerateJwks();
        }

        public string Kid { get; private set; }

        public SignatureProvider SignatureProvider { get; private set; }

        public Register.API.Infrastructure.Models.JsonWebKeySet JsonWebKeySet { get; private set; }

        private X509SecurityKey SecurityKey { get; set; }

        private X509SigningCredentials SigningCredentials { get; set; }

        private CDR.Register.API.Infrastructure.Models.JsonWebKeySet GenerateJwks()
        {
            var rsaParams = this.SigningCredentials.Certificate.GetRSAPublicKey().ExportParameters(false);
            var e = Base64UrlEncoder.Encode(rsaParams.Exponent);
            var n = Base64UrlEncoder.Encode(rsaParams.Modulus);

            var jwk = new CDR.Register.API.Infrastructure.Models.JsonWebKey()
            {
                Alg = this.SigningCredentials.Algorithm,
                Kid = this.SigningCredentials.Kid,
                Kty = this.SecurityKey.PublicKey.KeyExchangeAlgorithm,
                N = n,
                E = e,
                Key_ops = ["sign", "verify"],
            };

            return new CDR.Register.API.Infrastructure.Models.JsonWebKeySet()
            {
                Keys = [jwk],
            };
        }
    }
}
