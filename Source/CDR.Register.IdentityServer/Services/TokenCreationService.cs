using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.Security.Claims;
using CDR.Register.IdentityServer.Models;

namespace CDR.Register.IdentityServer.Services
{
    public class TokenCreationService : DefaultTokenCreationService
    {
        public TokenCreationService(
            ISystemClock clock,
            IKeyMaterialService keys,
            IdentityServerOptions options,
            ILogger<DefaultTokenCreationService> logger) : base(clock, keys, options, logger)
        {
        }

        public override async Task<string> CreateTokenAsync(Token token)
        {
            // Override the handling of the cnf claim as there is an issue in Identity Server 4.
            if (token.Type == "access_token" && !string.IsNullOrEmpty(token.Confirmation))
            {
                var cnf = token.Confirmation;
                token.Confirmation = null;
                token.Claims.Add(new Claim("cnf", cnf, JsonClaimValueTypes.Json));
            }

            var header = await CreateHeaderAsync(token);
            var payload = await CreatePayloadAsync(token);

            var jwt = new JwtSecurityToken(header, payload);
            return await CreateJwtAsync(jwt);
        }

    }
}
