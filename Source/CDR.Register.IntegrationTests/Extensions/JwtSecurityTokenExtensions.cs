using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;

#nullable enable

namespace CDR.Register.IntegrationTests.Extensions
{
    static public class JwtSecurityTokenExtensions
    {
        /// <summary>
        /// Get claim for claimType. Throws exception if no claim or multiple claims (ie must be a single claim for claimType).
        /// </summary>
        /// <returns></returns>
        static public Claim Claim(this JwtSecurityToken jwt, string claimType)
            => jwt.Claims.Where(claim => claim.Type == claimType).Single();

        /// <summary>
        /// Assert JWT contains a claim with given value.
        /// </summary>
        /// <param name="claimType">The claim type to assert</param>
        /// <param name="claimValue">The claim value to assert. If null then claim value can be anything (it is not checked)</param>
        /// <param name="optional">If true then the claim itself is optional and doesn't need to exist in the claims</param>
        static public void AssertClaim(this JwtSecurityToken jwt, string claimType, string? claimValue, bool optional = false)
        {
            var claims = jwt.Claims.Where(claim => claim.Type == claimType);

            // Claim not found and it's optional so just exit
            if (optional && (claims == null || !claims.Any()))
            {
                return;
            }

            claims.Should().NotBeNull(claimType);
            claims.Should().ContainSingle(claimType);

            // Check value value
            if (claimValue != null)
            {
                var claim = claims.First();
                claim.Value.Should().Be(claimValue, claimType);
            }
        }

        static public void AssertClaimIsArray(this JwtSecurityToken jwt, string claimType, string[]? claimValues)
        {
            var claims = jwt.Claims.Where(claim => claim.Type == claimType);

            claims.Should().NotBeNull(claimType);

            var claimsAsArray = claims.Select(claim => claim.Value);
            claimsAsArray.Should().BeEquivalentTo(claimValues);
        }
    }
}
