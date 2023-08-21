using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Class used to generate a private_key_jwt client assertion based on a provided private key.
    /// </summary>
    public class PrivateKeyJwt
    {

        public string PrivateKeyBase64 { get; private set; }
        private readonly string JtiClaim;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PrivateKeyJwt()
        {
        }

        /// <summary>
        /// Provide the primary key to the constructor.
        /// </summary>
        /// <param name="privateKey">Base64 encoded private key.</param>
        /// <remarks>
        /// If the privateKey contains the -----BEGIN PRIVATE KEY----- and -----END PRIVATE KEY----- markers then these will be stripped out.
        /// </remarks>
        public PrivateKeyJwt(string privateKey)
        {
            if (!string.IsNullOrEmpty(privateKey))
            {
                PrivateKeyBase64 = FormatKey(privateKey);
            }
        }

        /// <summary>
        /// Provide the Pkcs8 private key from X509 certificate.
        /// </summary>
        /// <param name="certFilePath">The path to the certificate.</param>
        /// <param name="pwd">The password of the certificate.</param>
        /// <param name="jtiClaim">The jti claim to use for the JWT. Usually a unique GUID.</param>
        public PrivateKeyJwt(string certFilePath, string pwd, string jtiClaim)
        {
            var cert = new X509Certificate2(certFilePath, pwd, X509KeyStorageFlags.Exportable);
            var rsa = cert.GetRSAPrivateKey();
            var pvtKeyBytes = rsa.ExportPkcs8PrivateKey();
            this.PrivateKeyBase64 = Convert.ToBase64String(pvtKeyBytes);
            JtiClaim = jtiClaim;
        }

        /// <summary>
        /// Load the private key from a file.
        /// </summary>
        /// <param name="filePath">Path to the private key file</param>
        public void LoadPrivateKeyFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath must be provided");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("filePath cannot be found");

            }

            var privateKey = File.ReadAllText(filePath);
            PrivateKeyBase64 = FormatKey(privateKey);
        }

        /// <summary>
        /// Generate the private_key_jwt using the provided private key.
        /// </summary>
        /// <param name="issuer">The issuer of the JWT, usually set to the softwareProductId</param>
        /// <param name="audience">The audience of the JWT, usually set to the target token endpoint</param>
        /// <param name="subject">The subject of the JWT, usually set to the softwareProductId</param>
        /// <returns>A base64 encoded JWT</returns>
        public string Generate(string issuer, string audience, string subject, string signingAlgorithm = SecurityAlgorithms.RsaSsaPssSha256)
        {
            if (string.IsNullOrEmpty(PrivateKeyBase64))
            {
                throw new ArgumentException("privateKey must be set");
            }

            if (string.IsNullOrEmpty(issuer))
            {
                throw new ArgumentException("issuer must be provided");
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new ArgumentException("audience must be provided");
            }

            var privateKeyBytes = Convert.FromBase64String(PrivateKeyBase64);

            using (var rsa = RSA.Create())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                var kid = GenerateKeyId(rsa);
                var privateSecurityKey = new RsaSecurityKey(rsa)
                {
                    KeyId = kid,
                    CryptoProviderFactory = new CryptoProviderFactory()
                    {
                        CacheSignatureProviders = false
                    }
                };

                var descriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Subject = new ClaimsIdentity(new List<Claim> { new Claim("sub", subject) }),
                    //SigningCredentials = new SigningCredentials(privateSecurityKey, SecurityAlgorithms.RsaSsaPssSha256),
                    SigningCredentials = new SigningCredentials(privateSecurityKey, signingAlgorithm),
                    NotBefore = null,
                    IssuedAt = null,
                    Claims = new Dictionary<string, object>()
                };

                if (!string.IsNullOrEmpty(JtiClaim))
                {
                    descriptor.Claims.Add("jti", JtiClaim);
                }

                var tokenHandler = new JsonWebTokenHandler();
                return tokenHandler.CreateToken(descriptor);
            }
        }

        /// <summary>
        /// Format the private key by removing the markers and newline characters,
        /// </summary>
        /// <param name="privateKey">Raw private key</param>
        /// <returns>Formatted private key</returns>
        private static string FormatKey(string privateKey)
        {
            return privateKey.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\r\n", "").Trim();
        }

        private static string GenerateKeyId(RSAParameters rsaParams, out string e, out string n)
        {
            e = Base64UrlEncoder.Encode(rsaParams.Exponent);
            n = Base64UrlEncoder.Encode(rsaParams.Modulus);
            var dict = new Dictionary<string, string>() {
                    {"e", e},
                    {"kty", "RSA"},
                    {"n", n}
                };
            var hash = SHA256.Create();
            var hashBytes = hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));
            return Base64UrlEncoder.Encode(hashBytes);
        }

        /// <summary>
        /// Generate the KeyId (kid) used in the header of a JWT or in a JWK.
        /// </summary>
        /// <param name="rsa">RSA</param>
        /// <returns>The generated kid value</returns>
        private static string GenerateKeyId(RSA rsa)
        {
            var rsaParameters = rsa.ExportParameters(false);
            return GenerateKeyId(rsaParameters, out _, out _);
        }
    }
}
