using CDR.Register.API.Infrastructure;
using CDR.Register.Domain.Entities;
using CDR.Register.Infosec.Interfaces;
using CDR.Register.Infosec.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CDR.Register.Infosec.Controllers
{
    [Route("idp/connect")]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IConfiguration _configuration;        
        private readonly ITokenService _tokenService;

        public TokenController(
            ILogger<TokenController> logger,
            IConfiguration configuration,
            IClientService clientService,
            ITokenService tokenService)
        {
            _logger = logger;
            _configuration = configuration;            
            _tokenService = tokenService;
        }

        [HttpPost("token", Name = "GetAccessToken")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> GetAccessToken([FromForm] ClientAssertionRequest clientAssertion)
        {
            var (isValid, error, errorDescription, client) = await Validate(clientAssertion, this.Request);
            if (!isValid)
            {
                return BadRequest(new ErrorResponse() { Error = error, ErrorDescription = errorDescription });
            }

            var expiry = _configuration.GetValue<int>("AccessTokenExpiryInSeconds", 300);
            var defaultScope = _configuration.GetValue<string>("DefaultScope", $"{Constants.Scopes.RegisterRead} {Constants.Scopes.RegisterBankRead}");
            var scope = clientAssertion.scope ?? defaultScope;
            var cnf = this.HttpContext.GetClientCertificateThumbprint(_configuration);

            return Ok(new AccessTokenResponse()
            {
                AccessToken = await _tokenService.CreateAccessToken(client, expiry, scope, cnf),
                ExpiresIn = expiry,
                Scope = scope,
                TokenType = "Bearer"
            });
        }

        private async Task<(bool isValid, string? error, string? errorDescription, SoftwareProductInfosec? client)> Validate(ClientAssertionRequest clientAssertion, HttpRequest request)
        {
            if (request.ContentType == null || !request.ContentType.Contains("application/x-www-form-urlencoded"))
            {
                return (false, "invalid_request", "Content-Type is not application/x-www-form-urlencoded", null);
            }

            // Basic validation.            
            if (string.IsNullOrEmpty(clientAssertion.grant_type))
            {
                return (false, "invalid_client", "grant_type not provided", null);
            }

            if (string.IsNullOrEmpty(clientAssertion.client_assertion))
            {
                return (false, "invalid_client", "client_assertion not provided", null);
            }

            if (string.IsNullOrEmpty(clientAssertion.client_assertion_type))
            {
                return (false, "invalid_client", "client_assertion_type not provided", null);
            }

            // Grant type needs to be client_credentials.
            if (!clientAssertion.grant_type.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "unsupported_grant_type", "grant_type must be client_credentials", null);
            }

            // Client assertion type needs to be urn:ietf:params:oauth:client-assertion-type:jwt-bearer.
            if (!clientAssertion.client_assertion_type.Equals("urn:ietf:params:oauth:client-assertion-type:jwt-bearer", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "invalid_client", "client_assertion_type must be urn:ietf:params:oauth:client-assertion-type:jwt-bearer", null);
            }

            // If scope is provided it must include "cdr-register:bank:read" or "cdr-register:read"
            if (clientAssertion.scope != null)
            {
                if (clientAssertion.scope.Trim().Length == 0)
                {
                    return (false, "invalid_client", $"empty scope", null);
                }

                var scopes = clientAssertion.scope.Split(' ');

                foreach (var scope in scopes)
                {
                    if (!scope.Equals(Constants.Scopes.RegisterRead) && !scope.Equals(Constants.Scopes.RegisterBankRead))
                    {
                        return (false, "invalid_client", $"invalid scope", null);
                    }
                }
            }
            else
            {
                if (request.Form.ContainsKey("scope"))
                {
                    return (false, "invalid_client", $"empty scope", null);
                }
            }

            // Code changes for client id optional 
            // The issuer of the client assertion is the client_id of the calling data recipient.
            // Need to extract the client_id (iss) from client assertion to load the client details.
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(clientAssertion.client_assertion))
            {
                return (false, "invalid_client", "Invalid client_assertion - token validation error", null);
            }

            var token = handler.ReadJwtToken(clientAssertion.client_assertion);
            if (token == null)
            {
                return (false, "invalid_client", "Invalid client_assertion - token validation error", null);
            }

            // Validate the client assertion.
            var clientAssertionResult = await _tokenService.ValidateClientAssertion(clientAssertion.client_id, clientAssertion.client_assertion);

            if (!clientAssertionResult.isValid)
            {
                return (false, "invalid_client", clientAssertionResult.message, null);
            }

            var client = clientAssertionResult.client;
            if (client == null)
            {
                return (false, "invalid_client", "invalid client_id", null);
            }

            if (!IsValidCertificate(client))
            {
                return (false, "invalid_client", "Client certificate validation failed", null);
            }

            return (true, null, null, client);
        }

        private bool IsValidCertificate(
            SoftwareProductInfosec client)
        {
            // Get the certificate thumbprint and common name from the headers.
            var thumbprint = this.HttpContext.GetClientCertificateThumbprint(_configuration);
            var httpHeaderCommonName = this.HttpContext.GetClientCertificateCommonName(_logger, _configuration);

            if (string.IsNullOrEmpty(thumbprint) || string.IsNullOrEmpty(httpHeaderCommonName))
            {
                _logger.LogError("Thumbprint {thumbprint} and/or common name {commonName} not found.", thumbprint, httpHeaderCommonName);
                return false;
            }

            // Find a matching cert for the software product client.
            var certs = client.X509Certificates.ToList();
            // Check if there is a matching cert with the provided common name (validating the thumbprint is not required)
            var matchingCert = certs.FirstOrDefault(c => 
                c.CommonName.GetCommonName().Equals(httpHeaderCommonName, StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation("Matching cert = {matchingCert}", matchingCert != null);

            return matchingCert != null;
        }
    }
}