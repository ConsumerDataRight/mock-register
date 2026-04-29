using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.SSA.API.Business
{
    /// <summary>
    /// The class extracts information from the certificate.
    /// </summary>
    public class CertificateService : ICertificateService
    {
        private readonly IMapper _mapper;

        public CertificateService(IConfiguration config, IMapper mapper)
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

            this._mapper = mapper;

            this.JsonWebKeySet = this.GenerateJwks();
        }

        public string Kid { get; private set; }

        public SignatureProvider SignatureProvider { get; private set; }

        public Register.API.Infrastructure.Models.JsonWebKeySet JsonWebKeySet { get; private set; }

        private X509SecurityKey SecurityKey { get; set; }

        private X509SigningCredentials SigningCredentials { get; set; }

        private Register.API.Infrastructure.Models.JsonWebKeySet GenerateJwks()
        {
            var rsaParams = this.SigningCredentials.Certificate.GetRSAPublicKey().ExportParameters(false);
            var key = new RsaSecurityKey(rsaParams);

            var jwkc = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
            jwkc.KeyOps.Add("verify");
            jwkc.Alg = this.SigningCredentials.Algorithm;
            jwkc.Kty = "RSA";
            jwkc.Kid = this.SigningCredentials.Certificate.Thumbprint;

            Register.API.Infrastructure.Models.JsonWebKey jwk = this._mapper.MapJwk(jwkc);

            return new Register.API.Infrastructure.Models.JsonWebKeySet()
            {
                Keys = [jwk],
            };
        }
    }
}
