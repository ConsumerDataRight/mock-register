using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CDR.Register.SSA.API.Business;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CDR.Register.SSA.API.UnitTests
{
    public class TokenizerServiceTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public TokenizerServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json")
                                .Build();

            this._configuration = configuration;

            var services = new ServiceCollection();

            services.AddSingleton<ICertificateService, CertificateService>(x => new CertificateService(this._configuration));
            services.AddSingleton<ITokenizerService, TokenizerService>();

            this._serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_Success()
        {
            // Arrange
            var tokenizerService = this._serviceProvider.GetRequiredService<ITokenizerService>();

            var ssa = new SoftwareStatementAssertionModel
            {
                Client_description = "Application to allow you to track your expenses",
                Client_name = "Track Xpense",
                Client_uri = "https://fintechx.io/products/trackxpense",
                Exp = 1619233821,
                Iat = 1619233221,
                Iss = "cdr-register",
                Jti = "cfa1cd02d7914be6b4d9c4d77c080ad7",
                Jwks_uri = "https://fintechx.io/products/trackxpense/jwks",
                Logo_uri = "https://fintechx.io/products/trackxpense/logo.png",
                Org_id = "20c0864b-ceef-4de0-8944-eb0962f825eb",
                Org_name = "Finance X",
                Recipient_base_uri = "https://fintechx.io",
                Redirect_uris = new List<string>()
                {
                    "https://fintechx.io/products/trackxpense/cb",
                },
                Revocation_uri = "https://fintechx.io/products/trackxpense/revoke",
                Scope = "openid profile common:customer.basic:read common:customer.detail:read bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:regular_payments:read bank:payees:read energy:accounts.basic:read energy:accounts.detail:read energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:billing:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.der:read energy:electricity.usage:read cdr:registration",
                Software_id = "9381dad2-6b68-4879-b496-c1319d7dfbc9",
                Software_roles = "data-recipient-software-product",
            };

            // Generate the SSA JWT token
            var ssaToken = await tokenizerService.GenerateJwtTokenAsync(ssa);

            var tokenHandler = new JwtSecurityTokenHandler();

            // Create the certificate which has only public key
            var cert = new X509Certificate2(this._configuration["SigningCertificatePublic:Path"]);

            // Get credentials from certificate
            var certificateSecurityKey = new X509SecurityKey(cert);

            // Set token validation parameters
            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = certificateSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "cdr-register",
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateAudience = false,
            };

            SecurityToken validatedToken;

            // Act
            var principal = tokenHandler.ValidateToken(ssaToken, validationParameters, out validatedToken);

            // Assert
            Assert.NotNull(validatedToken);
            Assert.Equal("cdr-register", validatedToken.Issuer);
            Assert.Equal(17, principal.Claims.Count());
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_InvalidToken_Failure()
        {
            // Arrange
            var tokenizerService = this._serviceProvider.GetRequiredService<ITokenizerService>();

            var ssa = new SoftwareStatementAssertionModel
            {
                Client_description = "Application to allow you to track your expenses",
                Client_name = "Track Xpense",
                Client_uri = "https://fintechx.io/products/trackxpense",
                Exp = 1619233821,
                Iat = 1619233221,
                Iss = "cdr-register",
                Jti = "cfa1cd02d7914be6b4d9c4d77c080ad7",
                Jwks_uri = "https://fintechx.io/products/trackxpense/jwks",
                Logo_uri = "https://fintechx.io/products/trackxpense/logo.png",
                Org_id = "20c0864b-ceef-4de0-8944-eb0962f825eb",
                Org_name = "Finance X",
                Recipient_base_uri = "https://fintechx.io",
                Redirect_uris = new List<string>()
                {
                    "https://fintechx.io/products/trackxpense/cb",
                },
                Revocation_uri = "https://fintechx.io/products/trackxpense/revoke",
                Scope = "openid profile common:customer.basic:read common:customer.detail:read bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:regular_payments:read bank:payees:read energy:accounts.basic:read energy:accounts.detail:read energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:billing:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.der:read energy:electricity.usage:read cdr:registration",
                Software_id = "9381dad2-6b68-4879-b496-c1319d7dfbc9",
                Software_roles = "data-recipient-software-product",
            };

            // Generate the SSA JWT token
            var ssaToken = await tokenizerService.GenerateJwtTokenAsync(ssa);

            // Create invalid token
            ssaToken = ssaToken.Replace('a', 'b');

            var tokenHandler = new JwtSecurityTokenHandler();

            // Create the certificate which has only public key
            var cert = new X509Certificate2(this._configuration["SigningCertificatePublic:Path"]);

            // Get credentials from certificate
            var certificateSecurityKey = new X509SecurityKey(cert);

            // Set token validation parameters
            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = certificateSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "cdr-register",
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateAudience = false,
            };

            try
            {
                SecurityToken validatedToken;

                // Act
                tokenHandler.ValidateToken(ssaToken, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message.Replace("\n", string.Empty);

                // Assert
                Assert.StartsWith("IDX10511: Signature validation failed.", errorMessage);
            }
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_InvalidCertificate_Failure()
        {
            // Arrange
            var tokenizerService = this._serviceProvider.GetRequiredService<ITokenizerService>();

            var ssa = new SoftwareStatementAssertionModel
            {
                Client_description = "Application to allow you to track your expenses",
                Client_name = "Track Xpense",
                Client_uri = "https://fintechx.io/products/trackxpense",
                Exp = 1619233821,
                Iat = 1619233221,
                Iss = "cdr-register",
                Jti = "cfa1cd02d7914be6b4d9c4d77c080ad7",
                Jwks_uri = "https://fintechx.io/products/trackxpense/jwks",
                Logo_uri = "https://fintechx.io/products/trackxpense/logo.png",
                Org_id = "20c0864b-ceef-4de0-8944-eb0962f825eb",
                Org_name = "Finance X",
                Recipient_base_uri = "https://fintechx.io",
                Redirect_uris = new List<string>()
                {
                    "https://fintechx.io/products/trackxpense/cb",
                },
                Revocation_uri = "https://fintechx.io/products/trackxpense/revoke",
                Scope = "openid profile common:customer.basic:read common:customer.detail:read bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:regular_payments:read bank:payees:read energy:accounts.basic:read energy:accounts.detail:read energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:billing:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.der:read energy:electricity.usage:read cdr:registration",
                Software_id = "9381dad2-6b68-4879-b496-c1319d7dfbc9",
                Software_roles = "data-recipient-software-product",
            };

            // Generate the SSA JWT token
            var ssaToken = await tokenizerService.GenerateJwtTokenAsync(ssa);

            var tokenHandler = new JwtSecurityTokenHandler();

            // Create the certificate which has only public key
            var cert = new X509Certificate2(this._configuration["InvalidSigningCertificatePublic:Path"]);

            // Get credentials from certificate
            var certificateSecurityKey = new X509SecurityKey(cert);

            // Set token validation parameters
            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = certificateSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "cdr-register",
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateAudience = false,
            };

            try
            {
                SecurityToken validatedToken;

                // Act
                tokenHandler.ValidateToken(ssaToken, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message.Replace("\n", string.Empty);

                // Assert
                Assert.StartsWith("IDX10503: Signature validation failed.", errorMessage);
            }
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ValidateJwks_Success()
        {
            // Arrange.
            var tokenizerService = this._serviceProvider.GetRequiredService<ITokenizerService>();
            var ssa = new SoftwareStatementAssertionModel
            {
                Client_description = "Application to allow you to track your expenses",
                Client_name = "Track Xpense",
                Client_uri = "https://fintechx.io/products/trackxpense",
                Exp = 1619233821,
                Iat = 1619233221,
                Iss = "cdr-register",
                Jti = "cfa1cd02d7914be6b4d9c4d77c080ad7",
                Jwks_uri = "https://fintechx.io/products/trackxpense/jwks",
                Logo_uri = "https://fintechx.io/products/trackxpense/logo.png",
                Org_id = "20c0864b-ceef-4de0-8944-eb0962f825eb",
                Org_name = "Finance X",
                Recipient_base_uri = "https://fintechx.io",
                Redirect_uris = new List<string>()
                {
                    "https://fintechx.io/products/trackxpense/cb",
                },
                Revocation_uri = "https://fintechx.io/products/trackxpense/revoke",
                Scope = "openid profile common:customer.basic:read common:customer.detail:read bank:accounts.basic:read bank:accounts.detail:read bank:transactions:read bank:regular_payments:read bank:payees:read energy:accounts.basic:read energy:accounts.detail:read energy:accounts.concessions:read energy:accounts.paymentschedule:read energy:billing:read energy:electricity.servicepoints.basic:read energy:electricity.servicepoints.detail:read energy:electricity.der:read energy:electricity.usage:read cdr:registration",
                Software_id = "9381dad2-6b68-4879-b496-c1319d7dfbc9",
                Software_roles = "data-recipient-software-product",
            };

            // Generate the SSA JWT token
            var ssaToken = await tokenizerService.GenerateJwtTokenAsync(ssa);
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ReadToken(ssaToken);

            // Get the certificate service based on config settings.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ssa.pfx");
            var inMemorySettings = new Dictionary<string, string>
            {
                { "SigningCertificate:Path", path },
                { "SigningCertificate:Password", "#M0ckRegister#" },
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var certificateService = new CertificateService(configuration);
            var jwks = certificateService.JsonWebKeySet;

            // Set token validation parameters
            var validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new JsonWebKey(jwks.Keys[0].ToJson()),
                ValidateIssuerSigningKey = true,
                ValidIssuer = "cdr-register",
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateAudience = false,
            };
            SecurityToken validatedToken;

            // Act.
            var principal = tokenHandler.ValidateToken(ssaToken, validationParameters, out validatedToken);

            // Assert
            Assert.NotNull(validatedToken);
            Assert.Equal("cdr-register", validatedToken.Issuer);
            Assert.Equal(17, principal.Claims.Count());
        }
    }
}
