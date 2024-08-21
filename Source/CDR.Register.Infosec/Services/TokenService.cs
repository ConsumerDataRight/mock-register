using CDR.Register.API.Infrastructure;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientService _clientService;

        public TokenService(
            ILogger<TokenService> logger,
            IConfiguration configuration,
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            IClientService clientService)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _clientService = clientService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client_id">client_id (form param) when provided and must match client assertion issuer and subject </param> 
        /// <param name="clientAssertion">clientAssertion </param> 
        /// <returns></returns>
        public async Task<(bool isValid, string? message, SoftwareProductInfosec? client)> ValidateClientAssertion(string? client_id, string clientAssertion)
        {
            JwtSecurityToken? validatedSecurityToken;
            SoftwareProductInfosec? client;
            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Validate the client assertion token.                        
                var invalidToken = handler.ReadJwtToken(clientAssertion);
                var clientId = invalidToken.Issuer;
                if (string.IsNullOrEmpty(clientId))
                {
                    return (false, "client_id is required", null);
                }

                // client_id (form param) when provided and must match client assertion issuer and subject
                if (!string.IsNullOrEmpty(client_id) &&
                     (!client_id.Equals(invalidToken.Issuer, StringComparison.OrdinalIgnoreCase) ||
                      !client_id.Equals(invalidToken.Subject, StringComparison.OrdinalIgnoreCase)
                     ))
                {
                    return (false, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id", null);
                }

                client = await _clientService.GetClientAsync(clientId);
                if (client == null)
                {
                    return (false, "invalid client_id", null);
                }

                var tokenValidationParameters = await BuildTokenValidationParameters(client);

                handler.ValidateToken(clientAssertion, tokenValidationParameters, out var token);

                validatedSecurityToken = (JwtSecurityToken)token;
                if (string.IsNullOrEmpty(validatedSecurityToken.Id))
                {
                    return (false, "Invalid client_assertion - 'jti' is required", null);
                }

                if (validatedSecurityToken.Header.Alg != SecurityAlgorithms.RsaSsaPssSha256 && validatedSecurityToken.Header.Alg != SecurityAlgorithms.EcdsaSha256)
                {
                    return (false, "Invalid client_assertion - Client assertion token signature algorithm must be PS256 or ES256", null);
                }

                if (!validatedSecurityToken.Subject.Equals(validatedSecurityToken.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Invalid client_assertion - 'sub' and 'iss' must have the same value", null);
                }

                if (!client.Id.Equals(validatedSecurityToken.Subject, StringComparison.OrdinalIgnoreCase) ||
                     !client.Id.Equals(validatedSecurityToken.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Invalid client_assertion - 'sub' and 'iss' must be set to the client_id", null);
                }

                // Has this jti already been used?
                if (IsBlacklisted(validatedSecurityToken.Issuer, validatedSecurityToken.Id))
                {
                    return (false, "Invalid client_assertion - 'jti' in the client assertion token must be unique", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid client_assertion - token validation error");
                return (false, "Invalid client_assertion - token validation error", null);
            }

            // Add the jti into the blacklist so that the same jti cannot be re-used until at least after it has expired.
            Blacklist(validatedSecurityToken.Issuer, validatedSecurityToken.Id, validatedSecurityToken.ValidTo.AddMinutes(5));

            return (true, null, client);
        }

        private async Task<TokenValidationParameters> BuildTokenValidationParameters(
            SoftwareProductInfosec client,
            int clockSkew = 120)
        {
            var validAudiences = BuildValidAudiences();

            return new TokenValidationParameters
            {
                ValidateIssuer = false,
                IssuerSigningKeys = (await GetClientKeys(client)),
                ValidateIssuerSigningKey = true,

                ValidAudiences = validAudiences,
                ValidateAudience = true,
                AudienceValidator = (IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                {

                    bool isValid = audiences.Any(audience => validationParameters.ValidAudiences.Contains(audience, StringComparer.OrdinalIgnoreCase));                    

                    if (!isValid)
                    {
                        string errorMessage = $"IDX10214: Audience validation failed. Audiences: '{string.Join(',', audiences)}'. Did not match: '{string.Join(',', validationParameters.ValidAudiences)}'.";
                        throw new SecurityTokenInvalidAudienceException(errorMessage)
                        {
                            InvalidAudience = string.Join(',', audiences)
                        };
                    }

                    return isValid;
                },

                RequireSignedTokens = true,
                RequireExpirationTime = true,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(clockSkew),
            };
        }

        private List<string> BuildValidAudiences()
        {
            var baseUri = _configuration.GetInfosecBaseUrl(_httpContextAccessor.HttpContext);
            var secureBaseUri = _configuration.GetInfosecBaseUrl(_httpContextAccessor.HttpContext, true);

            return new List<string>()
            {
                baseUri,
                $"{secureBaseUri}/connect/token"
            };
        }

        public async Task<string> CreateAccessToken(
            SoftwareProductInfosec client,
            int expiryInSeconds,
            string scope,
            string cnf)
        {
            var cert = await Task.Run(() => new X509Certificate2(_configuration.GetValue<string>("SigningCertificate:Path") ?? "", _configuration.GetValue<string>("SigningCertificate:Password"), X509KeyStorageFlags.Exportable));
            var signingCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSsaPssSha256);
            var issuer = _configuration.GetInfosecBaseUrl(_httpContextAccessor.HttpContext);

            List<Claim> claims = [new Claim("client_id", client.Id), 
                new Claim("jti", Guid.NewGuid().ToString()), 
                new Claim("scope", scope)];

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
                _logger.LogError(ex, "Error retrieving jwks from client {ClientId}", client.Id);
                return trustedKeys;
            }

            return trustedKeys;
        }

        public async Task<IList<JsonWebKey>> GetClientJwks(SoftwareProductInfosec client)
        {
            var handler = new HttpClientHandler();
            handler.SetServerCertificateValidation(_configuration);
            var httpClient = new HttpClient(handler);

            var passUserAgent = _configuration.GetValue<bool>("PassUserAgent"); //allows CTS to attach a header for request filtering
            if (passUserAgent)
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "mock-register");
            }

            var httpResponse = await httpClient.GetAsync(client.JwksUri);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("{Method}: {JwksUri} returned {StatusCode}", nameof(GetClientJwks), client.JwksUri, httpResponse.StatusCode);
                return new List<JsonWebKey>();
            }

            var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseContent);
            if (keys == null || keys.Keys == null || !keys.Keys.Any())
            {
                _logger.LogError("{Method}: No keys found at {JwksUri}", nameof(GetClientJwks), client.JwksUri);
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
