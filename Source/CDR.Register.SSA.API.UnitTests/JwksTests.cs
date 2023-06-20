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
            Assert.Equal("203A41DC1743F97212BFBF01B77895CD9F445BFB", jwk.kid);
            Assert.Equal("RSA", jwk.kty);
            Assert.Equal("sCKajSK266KlSW0sSOa3Jbfrq1PCa2EHkuXoNwC0VMhrq-u_J1qMxvM50LCT5OF45GWPl4LUFhoJlej-XQtHztBpB6NWX5eJXW45M2OHmerRqN9IP5oQ1yscTzyiQyFoTbLpjFyRASrQZy1XMGrMMa7tqLpyHDxzJX-SBsr_hq8Olj0LFLeWi3giLirj_4CRqqmTtvLCaMwGajpEGQz3Xc96FNZXUOIR-wX_WjbCzVn2-X7PHjgIbT_oURtnovxQ6ZXZRtqxBhIKwJ-zCOOZAAqDJcy-7QxtsWpU_IyRRPziAyQ254iLcjV125DgQnd5TsUQQX6nfBozgbYdSLfLzQ", jwk.n);
            Assert.Equal("AQAB", jwk.e);
            Assert.Equal(2, jwk.key_ops.Length);
        }

    }
}
