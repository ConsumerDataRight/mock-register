using CDR.Register.IdentityServer.Extensions;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Models;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace CDR.Register.IdentityServer.Configurations
{
    public class CdrSecretValidator : ISecretValidator
    {
        private readonly IJwkService _jwkService;
        private readonly ILogger<CdrSecretValidator> _logger;
        private readonly IdentityServerOptions _identityServerOptions;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public CdrSecretValidator(IdentityServerOptions identityServerOptions,
            IJwkService jwkService,
            IConfiguration configuration,
            ILogger<CdrSecretValidator> logger,
            IMediator mediator)
        {
            _identityServerOptions = identityServerOptions;
            _jwkService = jwkService;
            _configuration = configuration;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = new SecretValidationResult { Success = false };

            using (LogContext.PushProperty("MethodName", "ValidateAsync"))
            {
                _logger.LogInformation("Starting");
            }

            if (parsedSecret == null)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "604", null,
                    "Parsed Secret is null"), _logger);
                return fail;
            }

            if (parsedSecret.Type != Constants.ParsedSecretTypes.CdrSecret)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "605", null,
                    $"Cdr secret validator cannot process {parsedSecret.Type ?? "null"}"), _logger);
                return fail;
            }

            if (parsedSecret.Credential is not CdrCredential credential)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "606", null,
                    "ParsedSecret.Credential is not valid."), _logger);
                return fail;
            }

            // validate jwt
            List<SecurityKey> trustedKeys;
            try
            {
                trustedKeys = await secrets.GetJsonWebKeysAsync(_jwkService);
            }
            catch (Exception e)
            {
                using (LogContext.PushProperty("MethodName", "ValidateAsync"))
                {
                    _logger.LogError(e, "Trusted Keys Exception Error");
                }
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "607", null,
                    $"Could not parse secrets. {e.InnerException?.Message ?? e.Message}"), _logger);
                return fail;
            }

            if (!trustedKeys.Any())
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "608", null,
                    "There are no keys available to validate client assertion."), _logger);
                return fail;
            }

            if (!(await IsValidJwt(credential, trustedKeys, parsedSecret)))
            {
                return fail;
            }

            if (!(await IsValidCertificate(credential, secrets)))
            {
                return fail;
            }

            // Certificate validation passed so add the thumbprint of the certificate to the access token
            var values = new Dictionary<string, string>
            {
                { "x5t#S256", credential.CertificateThumbprintHeaderValues.First() }
            };

            var cnf = JsonConvert.SerializeObject(values);

            await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "0", null), _logger);

            var result = new SecretValidationResult
            {
                Success = true,
                Confirmation = cnf
            };

            using (LogContext.PushProperty("MethodName", "ValidateAsync"))
            {
                _logger.LogInformation("Finishing Result: {result}", result.Success);
            }

            return result;
        }

        private async Task<bool> IsValidJwt(
            CdrCredential credential,
            List<SecurityKey> trustedKeys,
            ParsedSecret parsedSecret)
        {
            var validAudience = _configuration.GetValue<string>(Constants.ConfigurationKeys.TokenUri);

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = trustedKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = parsedSecret.Id,
                ValidateIssuer = true,

                ValidAudience = string.IsNullOrEmpty(validAudience) ? _identityServerOptions.IssuerUri + "/connect/token" : validAudience,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(credential.Jwt, tokenValidationParameters, out var token);

                var jwtToken = (JwtSecurityToken)token;

                if (string.IsNullOrEmpty(jwtToken.Id))
                {
                    await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "709", null,
                        "'jti' in the client assertion token must have a value."), _logger);
                    return false;
                }

                var tokenIdentifier = $"{jwtToken.Audiences.First()}:{jwtToken.Id}";
                if (IsTokenBlacklisted(tokenIdentifier, jwtToken.ValidTo))
                {
                    await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "710", null,
                        "'jti' in the client assertion token must be unique."), _logger);
                    return false;
                }

                if (jwtToken.Subject != jwtToken.Issuer)
                {
                    await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "712", null,
                        "Both 'sub' and 'iss' in the client assertion token must have the same value."), _logger);
                    return false;
                }
            }
            catch (Exception e)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "711", null,
                    $"JWT token validation error. {e.Message}"), _logger);
                return false;
            }

            return true;
        }

        private async Task<bool> IsValidCertificate(
            CdrCredential credential,
            IEnumerable<Secret> secrets)
        {
            // validate certificate thumbprint
            if (StringValues.IsNullOrEmpty(credential.CertificateThumbprintHeaderValues))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "609", null,
                    "No thumbprint found in X509 client certificate."), _logger);
                return false;
            }

            if (credential.CertificateThumbprintHeaderValues.Count > 1)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "609", null,
                    $"Multiple thumbprints found in X509 client certificate."), _logger);
                return false;
            }

            string certThumbprint = credential.CertificateThumbprintHeaderValues.First();

            var certThumbprintSecretMatch = secrets.Any(s =>
                        !string.IsNullOrWhiteSpace(s.Type)
                        && s.Type.Equals(IdentityServerConstants.SecretTypes.X509CertificateThumbprint)
                        && !string.IsNullOrWhiteSpace(s.Value)
                        && s.Value.Equals(certThumbprint, StringComparison.OrdinalIgnoreCase));

            using (LogContext.PushProperty("MethodName", "ValidateAsync"))
            {
                _logger.LogInformation("X509 client certificate thumbprint '{certThumbprint}' match = {certThumbprintSecretMatch}", certThumbprint, certThumbprintSecretMatch);
            }

            if (!certThumbprintSecretMatch)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "611", null,
                    $"No matching x509 client certificate found for common name '{certThumbprint}'"), _logger);
                return false;
            }

            // validate certificate commonName
            if (StringValues.IsNullOrEmpty(credential.CertificateCommonNameHeaderValues))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "610", null,
                    "No common name found in X509 client certificate."), _logger);
                return false;
            }

            if (credential.CertificateCommonNameHeaderValues.Count > 1)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "610", null,
                    $"Multiple common names found in X509 client certificate"), _logger);
                return false;
            }

            string certCommonName = credential.CertificateCommonNameHeaderValues.First();

            var certNameSecretMatch = secrets.Any(s =>
                        !string.IsNullOrWhiteSpace(s.Type)
                        && s.Type.Equals(IdentityServerConstants.SecretTypes.X509CertificateName)
                        && !string.IsNullOrWhiteSpace(s.Value)
                        && s.Value.Equals(certCommonName, StringComparison.OrdinalIgnoreCase));

            using (LogContext.PushProperty("MethodName", "ValidateAsync"))
            {
                _logger.LogInformation("X509 client certificate common name '{commonName}' match = {certNameSecretMatch}", certCommonName, certNameSecretMatch);
            }

            // Certificate common name must match CRM record but thumbprint may be different as it can be an export
            // of an original issued certificate which will mean it will have a different thumbprint
            if (!certNameSecretMatch)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "611", null,
                    $"No matching x509 client certificate found for common name '{certCommonName}'"), _logger);
                return false;
            }

            return true;
        }

        private static bool IsTokenBlacklisted(string tokenId, DateTime expDate)
        {
            return false;
        }
    }
}
