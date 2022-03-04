using CDR.Register.Domain.Entities;
using CDR.Register.IdentityServer.Extensions;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Models;
using CDR.Register.Repository.Infrastructure;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace CDR.Register.IdentityServer.Configurations
{
    public class CdrClientStore : IClientStore
    {
        private readonly IClientService _clientService;
        private readonly IConfiguration _config;
        private readonly ILogger<CdrClientStore> _logger;
        private readonly IMediator _mediator;

        public CdrClientStore(
            IClientService clientService,
            IConfiguration config,
            ILogger<CdrClientStore> logger,
            IMediator mediator)
        {
            _config = config;
            _clientService = clientService;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = await _clientService.GetClientAsync(clientId);

            if (client == null)
            {
                await _mediator.LogErrorAndPublish(new NotificationMessage(GetType().Name, "800", null,
                    $"Client {clientId} is not found."), _logger);
                return null;
            }

            await _mediator.Publish(new NotificationMessage(GetType().Name, "0", null));

            return GetIdSvrClient(client);
        }

        private Client GetIdSvrClient(SoftwareProductIdSvr client)
        {
            // Create Certificate Thumbprint secrets
            var clientSectrets = client.X509Certificates.Select(cert => new Secret()
            {
                Type = SecretTypes.X509CertificateThumbprint,
                Value = cert.Thumbprint
            }).ToList();

            // Create Certificate Common Name secrets
            clientSectrets.AddRange(client.X509Certificates.Select(cert => new Secret()
            {
                Type = SecretTypes.X509CertificateName,
                Value = cert.CommonName
            }).ToList());

            // Create JWKS secret
            clientSectrets.Add(
                new Secret
                {
                    Type = Constants.SecretTypes.JwksUrl,
                    Value = client.JwksUri
                });

            var output = new Client
            {
                ClientId = client.Id,
                ClientName = client.Name,
                Enabled = true,

                // secret for authentication
                ClientSecrets = clientSectrets,

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // scopes that client has access to
                AllowedScopes = new List<string>
                {
                    StandardScopes.OpenId, CDSRegistrationScopes.BankRead, CDSRegistrationScopes.Read
                },

                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = _config.GetValue<int>("AccessTokenExpiryInSeconds", 300),
                IncludeJwtId = true
            };

            return output;
        }
    }
}
