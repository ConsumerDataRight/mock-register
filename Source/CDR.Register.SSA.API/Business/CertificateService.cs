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
        public string Kid { get; private set; }

        public SignatureProvider SignatureProvider { get; private set; }

        public Register.API.Infrastructure.Models.JsonWebKeySet JsonWebKeySet { get; private set; }

        private X509SecurityKey SecurityKey { get; set; }

        private X509SigningCredentials SigningCredentials { get; set; }

        public CertificateService(IConfiguration config)
        {
            //Create the certificate
            var cert = new X509Certificate2(config["SigningCertificate:Path"], config["SigningCertificate:Password"], X509KeyStorageFlags.Exportable);

            //Get credentials from certificate
            SecurityKey = new X509SecurityKey(cert);
            SigningCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);

            //Get certificate kid
            this.Kid = SigningCredentials.Kid;

            SigningCredentials.CryptoProviderFactory = new CryptoProviderFactory();

            //Get signature provider
            this.SignatureProvider = SigningCredentials.CryptoProviderFactory.CreateForSigning(SecurityKey, "PS256");

            this.JsonWebKeySet = GenerateJwks();
        }

        private CDR.Register.API.Infrastructure.Models.JsonWebKeySet GenerateJwks()
        {
            var rsaParams = this.SigningCredentials.Certificate.GetRSAPublicKey().ExportParameters(false);
            var e = Base64UrlEncoder.Encode(rsaParams.Exponent);
            var n = Base64UrlEncoder.Encode(rsaParams.Modulus);

            var jwk = new CDR.Register.API.Infrastructure.Models.JsonWebKey()
            {
                alg = this.SigningCredentials.Algorithm,
                kid = this.SigningCredentials.Kid,
                kty = this.SecurityKey.PublicKey.KeyExchangeAlgorithm,
                n = n,
                e = e,
                key_ops = ["sign", "verify"]
            };

            return new CDR.Register.API.Infrastructure.Models.JsonWebKeySet()
            {
                keys = [jwk]
            };
        }
    }
}
