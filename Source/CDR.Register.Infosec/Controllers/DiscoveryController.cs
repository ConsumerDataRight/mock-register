using System.Security.Cryptography.X509Certificates;
using CDR.Register.API.Infrastructure;
using CDR.Register.Infosec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.Infosec.Controllers
{
    [ApiController]
    [Route("idp/.well-known")]
    public class DiscoveryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DiscoveryController(
            IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpGet(Name = "GetOIDD")]
        [Route("openid-configuration")]
        public DiscoveryDocument Get()
        {
            var baseUrl = this._configuration.GetInfosecBaseUrl(this.HttpContext);
            var secureBaseUrl = this._configuration.GetInfosecBaseUrl(this.HttpContext, true);

            return new DiscoveryDocument()
            {
                Issuer = $"{baseUrl}",
                JwksUri = $"{baseUrl}/.well-known/openid-configuration/jwks",
                TokenEndpoint = $"{secureBaseUrl}/connect/token",
                ClaimsSupported = ["sub"],
                GrantTypesSupported = ["client_credentials"],
                IdTokenSigningAlgValuesSupported = ["PS256"],
                ResponseTypesSupported = ["token"],
                ScopesSupported = new string[] { "cdr-register:read" },
                CodeChallengeMethodsSupported = new string[] { "plain", "S256" },
                SubjectTypesSupported = new string[] { "public" },
                TlsClientCertificateBoundAccessTokens = true,
                TokenEndpointAuthMethodsSupported = new string[] { "private_key_jwt" },
                TokenEndpointAuthSigningAlgValuesSupported = new string[] { "PS256" },
            };
        }

        [HttpGet(Name = "GetJWKS")]
        [Route("openid-configuration/jwks")]
        public API.Infrastructure.Models.JsonWebKeySet? GetJwks()
        {
            var cert = new X509Certificate2(this._configuration.GetValue<string>("SigningCertificate:Path") ?? string.Empty, this._configuration.GetValue<string>("SigningCertificate:Password"), X509KeyStorageFlags.Exportable);
            var cert64 = Convert.ToBase64String(cert.RawData);
            var signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);
            var thumbprint = Base64UrlEncoder.Encode(cert.GetCertHash());
            var rsa = cert.GetRSAPublicKey();

            if (rsa != null)
            {
                var parameters = rsa.ExportParameters(false);
                var exponent = Base64UrlEncoder.Encode(parameters.Exponent ?? []);
                var modulus = Base64UrlEncoder.Encode(parameters.Modulus ?? []);

                var jwks = new API.Infrastructure.Models.JsonWebKeySet
                {
                    Keys =
                    [
                        new API.Infrastructure.Models.JsonWebKey
                        {
                            Kty = "RSA",
                            Use = "sig",
                            Kid = signingCredentials.Kid,
                            X5t = thumbprint,
                            E = exponent,
                            N = modulus,
                            X5c = [cert64],
                            Alg = "PS256",
                        }

                    ],
                };
                return jwks;
            }

            return null;
        }
    }
}
