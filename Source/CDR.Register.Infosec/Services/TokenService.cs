using CDR.Register.Domain.Entities;
using CDR.Register.Infosec.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace CDR.Register.Infosec.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger; 
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        public TokenService(
            ILogger<TokenService> logger,
            IConfiguration configuration,
            IDistributedCache cache)
        {
            _logger = logger; 
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<(bool isValid, string? message)> ValidateClientAssertion(SoftwareProductInfosec client, string clientAssertion)
        {
            // Validate the client assertion token.
            var validAudience = _configuration.GetValue<string>(Constants.ConfigurationKeys.TokenEndpoint);
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = (await GetClientKeys(client)),
                ValidateIssuerSigningKey = true,

                ValidateIssuer = false,

                ValidAudience = validAudience,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };
            JwtSecurityToken? validatedSecurityToken = null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(clientAssertion, tokenValidationParameters, out var token);

                validatedSecurityToken = (JwtSecurityToken)token;
                if (string.IsNullOrEmpty(validatedSecurityToken.Id))
                {
                    return (false, "Invalid client_assertion - 'jti' is required");
                }

                if (!client.Id.Equals(validatedSecurityToken.Subject, StringComparison.OrdinalIgnoreCase)
                 || !client.Id.Equals(validatedSecurityToken.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id");
                }

                if (!validatedSecurityToken.Subject.Equals(validatedSecurityToken.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Invalid client_assertion - 'sub' and 'iss' must have the same value");
                }

                // Has this jti already been used?
                if (IsBlacklisted(validatedSecurityToken.Issuer, validatedSecurityToken.Id))
                {
                    return (false, "Invalid client_assertion - 'jti' has already been used");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid client_assertion - token validation error");
                return (false, "Invalid client_assertion - token validation error");
            }

            // Add the jti into the blacklist so that the same jti cannot be re-used until at least after it has expired.
            Blacklist(validatedSecurityToken.Issuer, validatedSecurityToken.Id, validatedSecurityToken.ValidTo.AddMinutes(5));

            return (true, null);
        }

        public async Task<string> CreateAccessToken(
            SoftwareProductInfosec client, 
            int expiryInSeconds,
            string scope, 
            string cnf)
        {
            var cert = new X509Certificate2(_configuration.GetValue<string>("SigningCertificate:Path"), _configuration.GetValue<string>("SigningCertificate:Password"), X509KeyStorageFlags.Exportable);
            var signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);
            var issuer = _configuration.GetValue<string>(Constants.ConfigurationKeys.Issuer);

            var claims = new List<Claim>();
            claims.Add(new Claim("client_id", client.Id));
            claims.Add(new Claim("jti", Guid.NewGuid().ToString()));
            claims.Add(new Claim("scope", scope));

            claims.Add(new Claim(
                "cnf", 
                JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    { "x5t#S256", cnf }
                }), 
                JsonClaimValueTypes.Json));

            var jwtHeader = new JwtHeader(
                signingCredentials: signingCredentials, 
                outboundAlgorithmMap: null, 
                tokenType: "at+jwt");

            var jwtPayload = new JwtPayload(
                issuer: issuer, 
                audience: "cdr-register", 
                claims: claims, 
                notBefore: DateTime.UtcNow, 
                expires: DateTime.UtcNow.AddSeconds(expiryInSeconds), 
                issuedAt: DateTime.UtcNow);

            var jwt = new JwtSecurityToken(jwtHeader, jwtPayload);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwt);
        }

        public async Task<IList<SecurityKey>> GetClientKeys(SoftwareProductInfosec client)
        {
            // validate jwt
            var trustedKeys = new List<SecurityKey>();
            try
            {
                var jwks = await GetClientJwks(client);
                trustedKeys.AddRange(jwks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving jwks from client {clientId}", client.Id);
                return trustedKeys;
            }

            return trustedKeys;
        }

        public async Task<IList<JsonWebKey>> GetClientJwks(SoftwareProductInfosec client)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
            var httpClient = new HttpClient(handler);
            var httpResponse = await httpClient.GetAsync(client.JwksUri);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("{method}: {jwksUri} returned {statusCode}", nameof(GetClientJwks), client.JwksUri, httpResponse.StatusCode);
                return new List<JsonWebKey>();
            }

            var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseContent);
            if (keys == null || keys.Keys == null || !keys.Keys.Any())
            {
                _logger.LogError("{method}: No keys found at {jwksUri}", nameof(GetClientJwks), client.JwksUri);
                return new List<JsonWebKey>();
            }

            return keys.Keys;
        }

        public bool IsBlacklisted(string clientId, string id)
        {
            return !string.IsNullOrEmpty(_cache.GetString($"{clientId}::{id}"));
        }

        public void Blacklist(string clientId, string id, DateTime expiresOn)
        {
            try
            {
                _cache.SetString($"{clientId}::{id}", id, new DistributedCacheEntryOptions() { AbsoluteExpiration = new DateTimeOffset(expiresOn) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while adding security token to cache");
            }
        }
    }
}
