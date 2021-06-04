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
            Assert.Equal("542A9B91600488088CD4D816916A9F4488DD2651", jwk.kid);
            Assert.Equal("RSA", jwk.kty);
            Assert.Equal("0qE0TxxopsXaRNZ7qr9w0Sy0-kBGabAjB5BDn0ekvevomMFCvxdxw2d95aLc09k59cFH_dNsP0DCCE9ALp5Ob8gXNHcKEnKzFeLnZGo0xDSw6Xu-JuBw5-Z9bDfSWG_iSoPitG4Bk9j1S2brsKkOOa8iIE0JqkFMu___0ifoaA0C62c5QJIH8qvEr7x-zG9H9Bt1aFOznMy-TvI2s91otg4N_E1RKfRHnwPRjuw7fO1UCxn2LRLR0J_vz_uEHBs9CjH7KcPknPw43nmL-JDWQjUgM2FioTXDwhBEb5o_FQkLJlZmVrxg27iyAePYnoWDenGQsSS_2AhBAfZjqTIXBQ", jwk.n);
            Assert.Equal("AQAB", jwk.e);
            Assert.Equal(2, jwk.key_ops.Length);
        }

    }
}
