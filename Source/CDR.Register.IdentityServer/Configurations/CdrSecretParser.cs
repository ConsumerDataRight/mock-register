using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Extensions;
using CDR.Register.IdentityServer.Models;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace CDR.Register.IdentityServer.Configurations
{
    public class CdrSecretParser : ISecretParser
    {
        private readonly IdentityServerOptions _options;
        private readonly ILogger<CdrSecretParser> _logger;
        private readonly IMediator _mediator;

        public CdrSecretParser(IdentityServerOptions options,
            ILogger<CdrSecretParser> logger,
            IMediator mediator)
        {
            _options = options;
            _logger = logger;
            _mediator = mediator;
        }

        public string AuthenticationMethod => OidcConstants.EndpointAuthenticationMethods.PrivateKeyJwt;

        public async Task<ParsedSecret> ParseAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-TlsClientCertThumbprint", out StringValues certThumbprintHeaderValues))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "600", null,
                   "Request header X-TlsClientCertThumbprint not found."), _logger);
                return null;
            }

            if (!context.Request.Headers.TryGetValue("X-TlsClientCertCN", out StringValues certCommonNameHeaderValues))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "603", null,
                   "Request header X-TlsClientCertCN not found."), _logger);
                return null;
            }

            using (LogContext.PushProperty("MethodName", "ParseAsync"))
            {
                _logger.LogDebug("Start parsing for JWT client assertion in post body");
            }

            if (!context.Request.HasFormContentType)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "902", null,
                   "No JWT client assertion found in post body"), _logger);
                return null;
            }

            var body = context.Request.ReadFormAsync().Result;

            if (body == null)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "902", null,
                    "No JWT client assertion found in post body"), _logger);
                return null;
            }

            var clientId = body[OidcConstants.TokenRequest.ClientId].FirstOrDefault();
            var clientAssertionType = body[OidcConstants.TokenRequest.ClientAssertionType].FirstOrDefault();
            var clientAssertion = body[OidcConstants.TokenRequest.ClientAssertion].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(clientAssertion) || clientAssertionType != OidcConstants.ClientAssertionTypes.JwtBearer)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "902", null,
                    "No JWT client assertion found in post body"), _logger);
                return null;
            }

            if (clientAssertion.Length > _options.InputLengthRestrictions.Jwt)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "705", null,
                    "Client assertion token exceeds maximum length."), _logger);
                return null;
            }

            var assertionClientId = GetClientIdFromToken(clientAssertion);
            if (string.IsNullOrWhiteSpace(assertionClientId))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "706", null,
                    "Client assertion token sub claim (client_id) is missing."), _logger);
                return null;
            }

            if (assertionClientId.Length > _options.InputLengthRestrictions.ClientId)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "707", null,
                    "Client assertion token sub claim (client_id) exceeds maximum length."), _logger);
                return null;
            }

            if (!assertionClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase))
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "708", null,
                    "Client assertion token sub claim (client_id) does not match token request body client_id"), _logger);
                return null;
            }

            return new ParsedSecret
            {
                Id = assertionClientId,
                Credential = new CdrCredential { Jwt = clientAssertion, CertificateThumbprintHeaderValues = certThumbprintHeaderValues, CertificateCommonNameHeaderValues = certCommonNameHeaderValues },
                Type = Constants.ParsedSecretTypes.CdrSecret
            };
        }

        private string GetClientIdFromToken(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                return jwt.Subject;
            }
            catch (Exception e)
            {
                using (LogContext.PushProperty("MethodName", "GetClientIdFromToken"))
                {
                    _logger.LogWarning("Could not parse client assertion", e);
                }
                return null;
            }
        }
    }

    public class CdrCredential
    {
        public string Jwt { get; set; }
        public StringValues CertificateThumbprintHeaderValues { get; set; }
        public StringValues CertificateCommonNameHeaderValues { get; set; }
    }
}
