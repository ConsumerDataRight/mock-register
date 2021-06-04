using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using CDR.Register.IdentityServer.Configurations;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using static IdentityModel.OidcConstants;
using static IdentityServer4.IdentityServerConstants;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public static class JwkMock
    {
        public const string ValidClientId = "1C4E192C-AE59-EA11-A829-000D3A95C2C81";
        public const string ValidBrandCertificateName = "brand-cert-common-name";
        public const string ValidSoftwareProductCertificateName = "software-product-cert-common-name";

        public const string ValidCertificateThumbprint = "de5cc94d8f4b08f1429fc63f673304ae46d7865f9bd5f3c2fbba5e4abd3ee180";
        public const string BankScope = "cdr-register:bank:read";
        public const string GrantType = "client_credentials";
        public const string ContentType = "application/x-www-form-urlencoded";

        public const string SigningCertificatePath = "Certificates/dev-cts-dr-signing.pfx";
        public const string SigningCertificatePassword = "12345";
        public const string SigningCertificateJwks = "{ \"keys\": [ {\"e\":\"AQAB\",\"kid\":\"70fc38d403c347c1b6216e23cf3eb4df\",\"kty\":\"RSA\",\"n\":\"vPS0EyZJEHWc9q44P73uTsDSBjSqRYmT8kYMlPRlpqruUQ8d2wkVHBi9x_iI061ZBwNSbfgXE8SzCiGMD8rz_S1bQqw4Q0vZe-URHm1NlzmOKaKtgaOOuU1ztybuW1qWBb2hmHDh2msmaubn5GYsAikCPSjqAG0ZcpW1MUAtdrX6U351wm5oN_yJFtchhsvgHJY0XTSl7u1kbfe-Uhy2qU6Hd7xdSpo9wCWjz8EcOHmkMvLBHbWrhe_ZFD4UvIyycBKYvtyxdqE8Chhq3aCfL5urEhwmgQ-TIj9qGcnRDp--uUFtgEF6FWeqeGDXqKWglzrc-ea7UMm1FUEYr2ZmgQ\"}]}";

        public static readonly Client MockClient = new Client
        {
            ClientId = "1C4E192C-AE59-EA11-A829-000D3A95C2C81",
            ClientName = "ClientOne",
            Enabled = true,
            RedirectUris = new string[] { "https://localhost:test/auth", "https://localhost:auth" },
            ClientSecrets = new List<Secret>()
            {
                new Secret
                {
                    Type = "JWK",
                    Value = "{'e': 'AQAB','kty': 'RSA','n': 'vPS0EyZJEHWc9q44P73uTsDSBjSqRYmT8kYMlPRlpqruUQ8d2wkVHBi9x_iI061ZBwNSbfgXE8SzCiGMD8rz_S1bQqw4Q0vZe-URHm1NlzmOKaKtgaOOuU1ztybuW1qWBb2hmHDh2msmaubn5GYsAikCPSjqAG0ZcpW1MUAtdrX6U351wm5oN_yJFtchhsvgHJY0XTSl7u1kbfe-Uhy2qU6Hd7xdSpo9wCWjz8EcOHmkMvLBHbWrhe_ZFD4UvIyycBKYvtyxdqE8Chhq3aCfL5urEhwmgQ-TIj9qGcnRDp--uUFtgEF6FWeqeGDXqKWglzrc-ea7UMm1FUEYr2ZmgQ'}",
                },
                new Secret {
                    Type =  Constants.SecretTypes.JwksUrl,
                    Value = "https://localhost/jwks"
                },
                new Secret {
                    Type = SecretTypes.X509CertificateName,
                    Value = ValidBrandCertificateName
                },
                new Secret {
                    Type = SecretTypes.X509CertificateName,
                    Value = ValidSoftwareProductCertificateName
                }
            },
            AllowedGrantTypes = IdentityServer4.Models.GrantTypes.HybridAndClientCredentials,
            AllowedScopes = new List<string>
            {
                OidcConstants.StandardScopes.OpenId,
                BankScope,
            },
            Claims = new List<ClientClaim>()
            {
                new ClientClaim(ClientMetadata.IdentityTokenEncryptedResponseEncryption, CdrConstants.Algorithms.Jwe.Enc.A256GCM),
                new ClientClaim(ClientMetadata.IdentityTokenEncryptedResponseAlgorithm, CdrConstants.Algorithms.Jwe.Alg.RSAOAEP256),
            },
            AccessTokenType = AccessTokenType.Jwt,
            AccessTokenLifetime = 120,
            IncludeJwtId = true,
            RequireClientSecret = true,
            AllowOfflineAccess = true,
            RefreshTokenExpiration = TokenExpiration.Absolute,
            RefreshTokenUsage = TokenUsage.ReUse,
            ProtocolType = ProtocolTypes.OpenIdConnect,
        };

        public static string CreateValidMockClientAssertion(string clientId = ValidClientId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(clientId, claims, securityKey);

            return jwtToken;
        }

        public static ParsedSecret GetValidMockParsedSecret(string clientId = ValidClientId)
        {
            return new ParsedSecret
            {
                Id = clientId,
                Credential = new CdrCredential
                {
                    Jwt = CreateValidMockClientAssertion(clientId),
                    CertificateThumbprintHeaderValues = ValidCertificateThumbprint,
                    CertificateCommonNameHeaderValues = ValidBrandCertificateName
                },
                Type = Constants.ParsedSecretTypes.CdrSecret
            };
        }

        public static SigningCredentials GetMockSingleCredential()
        {
            using var certificate = new X509Certificate2(SigningCertificatePath, SigningCertificatePassword);
            var privateKey = certificate.GetRSAPrivateKey();
            var rsaSecurityKey = new RsaSecurityKey(privateKey);
            var credential = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSsaPssSha256);
            return credential;
        }

        public static X509SecurityKey GetX509SecurityKey()
        {
            var certificate = new X509Certificate2(SigningCertificatePath, SigningCertificatePassword);
            var securityKey = new X509SecurityKey(certificate);

            return securityKey;
        }

        public static string GetJwtToken(string clientId, IEnumerable<Claim> claims, X509SecurityKey securityKey, string audience = "http://localhost-api-unittest/connect/token")
        {
            var payload = new JwtPayload(clientId, audience, claims, null, DateTime.UtcNow.AddMinutes(2), null);
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
            var header = new JwtHeader(signingCredentials);
            var jwt = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.WriteToken(jwt);

            return jwtToken;
        }

        public static HeaderDictionary GetMockHeader(string clientCertThumbprint = "mock X-TlsClientCertThumbprint",
            string clientCertCN = "mock X-TlsClientCertCN",
            string contenttype = "mock X-TlsClientCertThumbprint"
            )
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "X-TlsClientCertThumbprint", new StringValues(clientCertThumbprint) },
                { "X-TlsClientCertCN", new StringValues(clientCertCN) },
                { "Content-Type", new StringValues(contenttype) },
            });

            if (string.IsNullOrEmpty(clientCertThumbprint))
            {
                headers.Remove("X-TlsClientCertThumbprint");
            }

            if (string.IsNullOrEmpty(clientCertCN))
            {
                headers.Remove("X-TlsClientCertCN");
            }

            if (string.IsNullOrEmpty(contenttype))
            {
                headers.Remove("Content-Type");
            }

            return headers;
        }

        public static IEnumerable<Secret> GetMockSecrets(string type = Constants.SecretTypes.JwksUrl, string value = "https://localhost/jwks")
        {
            return new List<Secret>
            {
                new Secret {
                    Type = type,
                    Value = value
                },
                new Secret {
                    Type = SecretTypes.X509CertificateName,
                    Value = ValidBrandCertificateName
                },
                new Secret {
                    Type = SecretTypes.X509CertificateName,
                    Value = ValidSoftwareProductCertificateName
                },
                new Secret
                {
                    Type = SecretTypes.X509CertificateThumbprint,
                    Value = ValidCertificateThumbprint
                }
            };
        }

        public static ParsedSecret GetMockParsedSecret(string id, string jwt)
        {
            return GetMockParsedSecret(id, jwt, "CdrSecret");
        }

        public static ParsedSecret GetMockParsedSecret(string id, string jwt, string type)
        {
            return GetMockParsedSecret(id, jwt, type, new StringValues("mock CertificateThumbprint"));
        }

        public static ParsedSecret GetMockParsedSecret(string id, string jwt, string type, StringValues certificateThumbprintHeaderValues)
        {
            return GetMockParsedSecret(id, jwt, type, certificateThumbprintHeaderValues, new StringValues("mock CertificateCommonName"));
        }


        public static ParsedSecret GetMockParsedSecret(string id, string jwt, string type,
            StringValues certificateThumbprintHeaderValues, StringValues certificateCommonNameHeaderValues)
        {
            return new ParsedSecret
            {
                Id = id,
                Type = "CdrSecret",
                Credential = new CdrCredential
                {
                    Jwt = jwt,
                    CertificateThumbprintHeaderValues = certificateThumbprintHeaderValues,
                    CertificateCommonNameHeaderValues = certificateCommonNameHeaderValues,
                },
            };
        }

        public static SecretValidationResult GetMockValidationResultSuccess()
        {
            var values = new Dictionary<string, string>
            {
                { "x5t#S256", JwkMock.ValidCertificateThumbprint }
            };
            var cnf = JsonConvert.SerializeObject(values);

            return new SecretValidationResult
            {
                Success = true,
                Confirmation = cnf
            };
        }

        public static SecretValidationResult GetMockValidationResultFailed()
        {
            var values = new Dictionary<string, string>
            {
                { "x5t#S256", JwkMock.ValidCertificateThumbprint }
            };
            var cnf = JsonConvert.SerializeObject(values);

            return new SecretValidationResult
            {
                Success = false,
                Confirmation = cnf
            };
        }

    }
}
