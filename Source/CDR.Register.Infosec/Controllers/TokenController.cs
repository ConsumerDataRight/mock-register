using System.IdentityModel.Tokens.Jwt;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Attributes;
using CDR.Register.Domain.Entities;
using CDR.Register.Infosec.Interfaces;
using CDR.Register.Infosec.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using static CDR.Register.Domain.Constants;

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
            this._logger = logger;
            this._configuration = configuration;
            this._tokenService = tokenService;
        }

        [HttpPost("token", Name = "GetAccessToken")]
        [ValidateContentTypeFilter("application/x-www-form-urlencoded")]
        public async Task<IActionResult> GetAccessToken([FromForm] ClientAssertionRequest clientAssertion)
        {
            var (isValid, error, errorDescription, client) = await this.Validate(clientAssertion);
            if (!isValid || client == null)
            {
                return this.BadRequest(new ErrorResponse() { Error = error, ErrorDescription = errorDescription });
            }

            var expiry = this._configuration.GetValue<int>("AccessTokenExpiryInSeconds", 300);
            var defaultScope = this._configuration.GetValue<string>("DefaultScope") ?? $"{Constants.Scopes.RegisterRead}";
            var scope = clientAssertion.Scope ?? defaultScope;
            var cnf = this.HttpContext.GetClientCertificateThumbprint(this._configuration);

            return this.Ok(new AccessTokenResponse()
            {
                AccessToken = await this._tokenService.CreateAccessToken(client, expiry, scope, cnf),
                ExpiresIn = expiry,
                Scope = scope,
                TokenType = JwtBearerDefaults.AuthenticationScheme,
            });
        }

        private static (bool IsValid, string? Error, string? ErrorDescription, SoftwareProductInfosec? Client) ValidateBasicParameters(ClientAssertionRequest clientAssertion)
        {
            if (string.IsNullOrEmpty(clientAssertion.Grant_type))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "grant_type not provided", null);
            }

            if (string.IsNullOrEmpty(clientAssertion.Client_assertion))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "client_assertion not provided", null);
            }

            if (string.IsNullOrEmpty(clientAssertion.Client_assertion_type))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "client_assertion_type not provided", null);
            }

            return (true, null, null, null);
        }

        private static (bool IsValid, string? Error, string? ErrorDescription, SoftwareProductInfosec? Client) ValidateScope(string? scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "empty scope", null);
            }

            var scopes = scope.Split(' ');
            foreach (var s in scopes)
            {
                if (!s.Equals(Constants.Scopes.RegisterRead))
                {
                    return (false, ErrorCodes.Generic.InvalidClient, "invalid scope", null);
                }
            }

            return (true, null, null, null);
        }

        private static (bool IsValid, string? Error, string? ErrorDescription, SoftwareProductInfosec? Client) ValidateClientAssertionToken(string? clientAssertion)
        {
            var handler = new JwtSecurityTokenHandler();

            if (clientAssertion == null || !handler.CanReadToken(clientAssertion))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "Invalid client_assertion - token validation error", null);
            }

            var token = handler.ReadJwtToken(clientAssertion);
            if (token == null)
            {
                return (false, ErrorCodes.Generic.InvalidClient, "Invalid client_assertion - token validation error", null);
            }

            return (true, null, null, null);
        }

        private async Task<(bool IsValid, string? Error, string? ErrorDescription, SoftwareProductInfosec? Client)> Validate(ClientAssertionRequest clientAssertion)
        {
            // Basic validation.
            var basicValidationResult = ValidateBasicParameters(clientAssertion);
            if (!basicValidationResult.IsValid)
            {
                return basicValidationResult;
            }

            // Grant type needs to be client_credentials.
            if (clientAssertion.Grant_type != null && !clientAssertion.Grant_type.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "unsupported_grant_type", "grant_type must be client_credentials", null);
            }

            // Client assertion type needs to be urn:ietf:params:oauth:client-assertion-type:jwt-bearer.
            if (clientAssertion.Client_assertion_type != null && !clientAssertion.Client_assertion_type.Equals("urn:ietf:params:oauth:client-assertion-type:jwt-bearer", StringComparison.OrdinalIgnoreCase))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "client_assertion_type must be urn:ietf:params:oauth:client-assertion-type:jwt-bearer", null);
            }

            // If scope is provided it must include "cdr-register:read"
            var scopeValidationResult = ValidateScope(clientAssertion.Scope);
            if (!scopeValidationResult.IsValid)
            {
                return scopeValidationResult;
            }

            // Code changes for client id optional
            // The issuer of the client assertion is the client_id of the calling data recipient.
            // Need to extract the client_id (iss) from client assertion to load the client details.
            var tokenValidationResult = ValidateClientAssertionToken(clientAssertion.Client_assertion);
            if (!tokenValidationResult.IsValid)
            {
                return tokenValidationResult;
            }

            // Validate the client assertion.
            var clientAssertionResult = await this._tokenService.ValidateClientAssertion(clientAssertion.Client_id ?? string.Empty, clientAssertion.Client_assertion ?? string.Empty);

            if (!clientAssertionResult.IsValid)
            {
                return (false, ErrorCodes.Generic.InvalidClient, clientAssertionResult.Message, null);
            }

            var client = clientAssertionResult.Client;
            if (client == null)
            {
                return (false, ErrorCodes.Generic.InvalidClient, "invalid client_id", null);
            }

            if (!this.IsValidCertificate(client))
            {
                return (false, ErrorCodes.Generic.InvalidClient, "Client certificate validation failed", null);
            }

            return (true, null, null, client);
        }

        private bool IsValidCertificate(
            SoftwareProductInfosec client)
        {
            // Get the certificate thumbprint and common name from the headers.
            var thumbprint = this.HttpContext.GetClientCertificateThumbprint(this._configuration);
            var httpHeaderCommonName = this.HttpContext.GetClientCertificateCommonName(this._logger, this._configuration);

            if (string.IsNullOrEmpty(thumbprint) || string.IsNullOrEmpty(httpHeaderCommonName))
            {
                this._logger.LogError("Thumbprint {Thumbprint} and/or common name {CommonName} not found.", thumbprint, httpHeaderCommonName);
                return false;
            }

            // Find a matching cert for the software product client.
            var certs = client.X509Certificates.ToList();

            // Check if there is a matching cert with the provided common name (validating the thumbprint is not required)
            var matchingCert = certs.Find(c =>
                c.CommonName.GetCommonName().Equals(httpHeaderCommonName, StringComparison.OrdinalIgnoreCase));

            this._logger.LogInformation("Matching cert = {MatchingCert}", matchingCert != null);

            return matchingCert != null;
        }
    }
}
