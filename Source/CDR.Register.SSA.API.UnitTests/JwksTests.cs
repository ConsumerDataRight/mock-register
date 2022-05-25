using System.Collections.Generic;
using System.IO;
using CDR.Register.SSA.API.Business;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CDR.Register.SSA.API.UnitTests
{
    public class JwksTests
    {
        [Fact]
        public void GenerateJwks_ValidCertificate_ShouldGenerateJwks()
        {
            // Arrange.
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ssa.pfx");
            var inMemorySettings = new Dictionary<string, string> {
                            {"SigningCertificate:Path", path},
                            {"SigningCertificate:Password", "#M0ckRegister#"},
                        };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var certificateService = new CertificateService(configuration);

            // Act.
            var jwks = certificateService.JsonWebKeySet;

            // Assert.
            Assert.NotNull(jwks);
            Assert.Single(jwks.keys);

            var jwk = jwks.keys[0];
            Assert.Equal("PS256", jwk.alg);
            Assert.Equal("F4EA299C607947E459AC47E69F7289F174B5B4DF", jwk.kid);
            Assert.Equal("RSA", jwk.kty);
            Assert.Equal("sMMObx8cM9OnP9IXUJ4jHuC95-xLkwQ-0qtXgDwEW3KszNMCFMokcuGbqhWeOpY0rGkZEVfD25Wr1rsStgbD4b1LhcMAFF63O_hQAtaj3-Lnv4MVSUBH_e-y4sxpxhgx_iX1e1Ycvq3I57JLNfT9_8MGg-pasSjUVeAFYi59zX4-pC55cBM25uTT7Th6JoZq9-El8-W2eOtmjIFRbYzrj1abmjpInLkMVTzGNviqRzuBVCwMscea4_Xk1eHY7ZYb2T0fcXSmMrbgwfXOVQDSZJy5kDVJJqqwsTWYIpaPBkc8vsTDWTfoZ9bjrs56bqUkC0pqICBGP7H58TEXJ-rLRw", jwk.n);
            Assert.Equal("AQAB", jwk.e);
            Assert.Equal(2, jwk.key_ops.Length);
        }

    }
}
