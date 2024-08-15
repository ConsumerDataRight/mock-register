using CDR.Register.API.Infrastructure;
using CDR.Register.Infosec.Models;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

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
            _configuration = configuration;
        }

        [HttpGet(Name = "GetOIDD")]
        [Route("openid-configuration")]
        public DiscoveryDocument Get()
        {
            var baseUrl = _configuration.GetInfosecBaseUrl(HttpContext);
            var secureBaseUrl = _configuration.GetInfosecBaseUrl(HttpContext, true);

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
            var cert = new X509Certificate2(_configuration.GetValue<string>("SigningCertificate:Path") ?? "", _configuration.GetValue<string>("SigningCertificate:Password"), X509KeyStorageFlags.Exportable);
            var cert64 = Convert.ToBase64String(cert.RawData);
            var signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);
            var thumbprint = Base64Url.Encode(cert.GetCertHash());
            var rsa = cert.GetRSAPublicKey();

            if (rsa != null)
            {
                var parameters = rsa.ExportParameters(false);
                var exponent = Base64Url.Encode(parameters.Exponent ?? []);
                var modulus = Base64Url.Encode(parameters.Modulus ?? []);

                var jwks = new API.Infrastructure.Models.JsonWebKeySet
                {
                    keys =
                    [
                        new API.Infrastructure.Models.JsonWebKey
                        {
                            kty = "RSA",
                            use = "sig",
                            kid = signingCredentials.Kid,
                            x5t = thumbprint,
                            e = exponent,
                            n = modulus,
                            x5c = [cert64],
                            alg = "PS256"
                        }
                    ]
                };
                return jwks;
            }
            return null;
        }
    }
}