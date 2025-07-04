﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CDR.Register.API.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CDR.Register.Admin.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiVersionNeutral]
    public class LoopbackController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LoopbackController(IConfiguration config)
        {
            this._config = config;
        }

        /// <summary>
        /// This controller action provides an implementation of a JWKS for a Mock Data Recipient.
        /// </summary>
        /// <returns>JWKS.</returns>
        [HttpGet]
        [Route("MockDataRecipientJwks")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public IActionResult MockDataRecipientJwks()
        {
            var cert = new X509Certificate2("Certificates/client.pem");
            var key = cert.GetRSAPublicKey();
            var rsaParams = key!.ExportParameters(false);
            var kid = GenerateKid(rsaParams, out var e, out var n);
            var jwk = new CDR.Register.API.Infrastructure.Models.JsonWebKey()
            {
                Alg = "PS256",
                Kid = kid,
                Kty = "RSA",
                N = n,
                E = e,
                Key_ops = ["sign", "verify"],
            };

            return new OkObjectResult(new CDR.Register.API.Infrastructure.Models.JsonWebKeySet()
            {
                Keys = [jwk],
            });
        }

        /// <summary>
        /// This controller action produces a client assertion for a mock data recipient.
        /// </summary>
        /// <returns>Client assertion string.</returns>
        /// <remarks>
        /// This client assertion can then be used in a private key jwt request.
        /// </remarks>
        [HttpGet]
        [Route("MockDataRecipientClientAssertion")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public IActionResult MockDataRecipientClientAssertion([FromQuery(Name = "iss")] string? iss = null, [FromQuery(Name = "aud")] string? aud = null)
        {
            var privateKeyRaw = System.IO.File.ReadAllText("Certificates/client.key");
            var privateKey = privateKeyRaw.Replace("-----BEGIN PRIVATE KEY-----", string.Empty).Replace("-----END PRIVATE KEY-----", string.Empty).Replace("\r\n", string.Empty).Trim();
            var privateKeyBytes = Convert.FromBase64String(privateKey);

            string audience = this._config.GetValue<string>("IdentityServerTokenUri") ?? "https://localhost:7001/idp/connect/token";
            string softwareProductId = this._config.GetValue<string>("LoopbackDefaultSoftwareProductId") ?? "6F7A1B8E-8799-48A8-9011-E3920391F713";

            softwareProductId = iss ?? softwareProductId;
            audience = aud ?? audience;

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            var kid = GenerateKid(rsa);
            var privateSecurityKey = new RsaSecurityKey(rsa)
            {
                KeyId = kid,
                CryptoProviderFactory = new CryptoProviderFactory()
                {
                    CacheSignatureProviders = false,
                },
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
                Claims = new Dictionary<string, object>(),
            };
            descriptor.Claims.Add("jti", Guid.NewGuid().ToString());

            var tokenHandler = new JsonWebTokenHandler();
            return new OkObjectResult(tokenHandler.CreateToken(descriptor));
        }

        /// <summary>
        /// This controller action produces a self signed JWT for the mock register.
        /// </summary>
        /// <returns>Self Signed JWT.</returns>
        [HttpGet]
        [Route("RegisterSelfSignedJwt")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public IActionResult RegisterSelfSignedJwt(
            [FromQuery] string aud)
        {
            var cert = new X509Certificate2(this._config.GetValue<string>("SigningCertificate:Path") ?? string.Empty, this._config.GetValue<string>("SigningCertificate:Password"), X509KeyStorageFlags.Exportable);
            var signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = "cdr-register",
                Subject = new ClaimsIdentity(new List<Claim> { new Claim("sub", "cdr-register") }),
                Audience = aud,
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = signingCredentials,
                NotBefore = null,
                IssuedAt = DateTime.UtcNow,
                Claims = new Dictionary<string, object>(),
            };
            descriptor.Claims.Add("jti", Guid.NewGuid().ToString());

            var tokenHandler = new JsonWebTokenHandler();
            return new OkObjectResult(tokenHandler.CreateToken(descriptor));
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
            var dict = new Dictionary<string, string>()
            {
                { "e", e },
                { "kty", "RSA" },
                { "n", n },
            };
            var hash = SHA256.Create();
            var hashBytes = hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dict)));
            return Base64UrlEncoder.Encode(hashBytes);
        }
    }
}
