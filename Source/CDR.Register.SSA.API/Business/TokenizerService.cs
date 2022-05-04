using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CDR.Register.SSA.API.Business
{
    /// <summary>
    /// The class generates a JWT software statement assertion token from data and a certificate.
    /// </summary>
    public class TokenizerService : ITokenizerService
    {
        private readonly ICertificateService _certificateService;

        public TokenizerService(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        /// <summary>
        /// Generate JWT token from software statement assertion data.
        /// </summary>
        /// <typeparam name="T">The software statement assertion model type.</typeparam>
        /// <param name="ssa">The software statement assertion.</param>
        /// <returns>The software statement assertion token as a string.</returns>
        public async Task<string> GenerateJwtTokenAsync<T>(T ssa)
            where T : SoftwareStatementAssertionModel
        {
            if (ssa == null)
            {
                return null;
            }

            var signingKid = _certificateService.Kid;

            // Create the JWT header
            var jwtHeader = JsonConvert.SerializeObject(new Dictionary<string, string>()
                {
                    { JwtHeaderParameterNames.Alg, "PS256" },
                    { JwtHeaderParameterNames.Kid, signingKid },
                    { JwtHeaderParameterNames.Typ, "JWT" }
                });

            var jwtEncodedHeader = Base64UrlEncoder.Encode(jwtHeader);

            // Encode the SSA as base64 for the JWT payload
            var jwtEncodedPayload = Base64UrlEncoder.Encode(ssa.ToJson());

            var byteData = Encoding.UTF8.GetBytes(jwtEncodedHeader + "." + jwtEncodedPayload);

            var jwtSignature = _certificateService.SignatureProvider.Sign(byteData);

            var jwtEncodedSignature = Base64UrlEncoder.Encode(jwtSignature);

            // Build the JWT (header.payload.signature)
            var jwt = $"{jwtEncodedHeader}.{jwtEncodedPayload}.{jwtEncodedSignature}";

            return await Task.FromResult(jwt);
        }
    }
}
