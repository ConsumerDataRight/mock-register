using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CDR.Register.Admin.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoopbackController : Controller
    {

        private readonly ILogger<LoopbackController> _logger;
        private readonly IConfiguration _config;

        public LoopbackController(ILogger<LoopbackController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// This controller action provides an implementation of a JWKS for a Mock Data Recipient.
        /// </summary>
        /// <returns>JWKS</returns>
        [HttpGet]
        [Route("MockDataRecipientJwks")]
        public IActionResult MockDataRecipientJwks()
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation("Received request for Mock Data Recipient JWKS...");
            }
            var cert = new X509Certificate2("Certificates/client.pem");
            var key = (RSA)cert.PublicKey.Key;
            var rsaParams = key.ExportParameters(false);
            var kid = GenerateKid(rsaParams, out var e, out var n);
            var jwk = new CDR.Register.API.Infrastructure.Models.JsonWebKey()
            {
                alg = "PS256",
                kid = kid,
                kty = "RSA",
                n = n,
                e = e,
                key_ops = new string[] { "sign", "verify" }
            };

            return Ok(new CDR.Register.API.Infrastructure.Models.JsonWebKeySet()
            {
                keys = new CDR.Register.API.Infrastructure.Models.JsonWebKey[] { jwk }
            });
        }

        /// <summary>
        /// This controller action produces a client assertion for a mock data recipient.
        /// </summary>
        /// <returns>Client assertion string</returns>
        /// <remarks>
        /// This client assertion can then be used in a private key jwt request.
        /// </remarks>
        [HttpGet]
        [Route("MockDataRecipientClientAssertion")]
        public IActionResult MockDataRecipientClientAssertion()
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation("Received request for Mock Data Recipient Client Assertion...");
            }
            var privateKeyRaw = System.IO.File.ReadAllText("Certificates/client.key");
            var privateKey = privateKeyRaw.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\r\n", "").Trim();
            var privateKeyBytes = Convert.FromBase64String(privateKey);

            string audience = _config.GetValue<string>("IdentityServerTokenUri") ?? "https://localhost:7001/idp/connect/token";
            string softwareProductId = _config.GetValue<string>("LoopbackDefaultSoftwareProductId") ?? "6F7A1B8E-8799-48A8-9011-E3920391F713";
            if (Request.Query.TryGetValue("iss", out var iss))
            {
                softwareProductId = iss.ToString();
            }

            using (var rsa = RSA.Create())
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                var kid = GenerateKid(rsa);
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
                    Issuer = softwareProductId,
                    Audience = audience,
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    Subject = new ClaimsIdentity(new List<Claim> { new Claim("sub", softwareProductId) }),
                    SigningCredentials = new SigningCredentials(privateSecurityKey, SecurityAlgorithms.RsaSsaPssSha256),
                    NotBefore = null,
                    IssuedAt = null,
                    Claims = new Dictionary<string, object>()
                };
                descriptor.Claims.Add("jti", Guid.NewGuid().ToString());

                var tokenHandler = new JsonWebTokenHandler();
                return new OkObjectResult(tokenHandler.CreateToken(descriptor));
            }
        }

        private static string GenerateKid(RSA rsa)
        {
            var rsaParameters = rsa.ExportParameters(false);
            return GenerateKid(rsaParameters, out _, out _);
        }

        private static string GenerateKid(RSAParameters rsaParams, out string e, out string n)
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
    }
}