using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Caching;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CDR.Register.IdentityServer.Models;
using System.Linq;
using IdentityServer4;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class CdrClientStoreUnitTests
    {
        private readonly IClientService _clientService = Substitute.For<IClientService>();
        private readonly ILogger<CdrClientStore> _logger = Substitute.For<ILogger<CdrClientStore>>();
        private readonly IConfiguration _config = Substitute.For<IConfiguration>();
        private readonly IMediator _mediator = Substitute.For<IMediator>();

        private CdrClientStore _cdrClientStore;

        public CdrClientStoreUnitTests()
        {
            _cdrClientStore = new CdrClientStore(_clientService, _logger, _config, _mediator);
        }

        [Fact]
        public async Task Null_Client_Should_Raise_NotificationMessage800()
        {
            //Arrange
            var clientId = Guid.NewGuid().ToString();
            _clientService.GetClientAsync(clientId).Returns((CrmClient)null);

            //Act
            await _cdrClientStore.FindClientByIdAsync(clientId);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "800"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "800"));
        }

        [Fact]
        public async Task FoundClient_Should_Raise_NotificationMessage0()
        {
            //Arrange
            var clientId = Guid.NewGuid().ToString();
            var crmClient = new CrmClient
            {
                Id = Guid.NewGuid().ToString(),
                X509Certificates = new List<CrmClientCertificate>
                 {
                     new CrmClientCertificate
                     {
                          CommonName = "mock common name",
                           Thumbprint = "mock Thumbprint",
                     }
                 },
                JwksUri = "http://localhost",
                Name = "mock name",
            };
            _clientService.GetClientAsync(clientId).Returns(crmClient);
            _config.GetSection("AccessTokenLifetimeSeconds").Value.Returns("3600");

            //Act
            await _cdrClientStore.FindClientByIdAsync(clientId);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
        }

        [Fact]
        public async Task Client_Secrets_Return_Multiple_Certs()
        {
            //Arrange
            var clientId = Guid.NewGuid().ToString();
            var crmClient = new CrmClient
            {
                Id = Guid.NewGuid().ToString(),
                X509Certificates = new List<CrmClientCertificate>
                 {
                     new CrmClientCertificate
                     {
                          CommonName = "brand-cert-common-name",
                          Thumbprint = "87af1214f736051a9e14baf0f205862691cce164"
                     },
                     new CrmClientCertificate
                     {
                          CommonName = "software-product-cert-common-name",
                          Thumbprint = "0456c7e4c3ecbfc45af12f562be4f991c9c52b03"
                     }

                 },
                JwksUri = "http://localhost/jwks",
                Name = "Mock Data Recipient Brand",
            };
            _clientService.GetClientAsync(clientId).Returns(crmClient);
            _config.GetSection("AccessTokenLifetimeSeconds").Value.Returns("3600");

            //Act
            var client = await _cdrClientStore.FindClientByIdAsync(clientId);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
            Assert.NotNull(client);
            Assert.NotNull(client.ClientSecrets);
            Assert.True(client.ClientSecrets.Where(s =>
                            !string.IsNullOrWhiteSpace(s.Type)
                            && s.Type.Equals(IdentityServerConstants.SecretTypes.X509CertificateName)
                            && !string.IsNullOrWhiteSpace(s.Value)
                            && s.Value.Equals("brand-cert-common-name", StringComparison.OrdinalIgnoreCase)).Any(),
                        "X509CertificateName not found for brand-cert-common-name");
            Assert.True(client.ClientSecrets.Where(s =>
                            !string.IsNullOrWhiteSpace(s.Type)
                            && s.Type.Equals(IdentityServerConstants.SecretTypes.X509CertificateName)
                            && !string.IsNullOrWhiteSpace(s.Value)
                            && s.Value.Equals("software-product-cert-common-name", StringComparison.OrdinalIgnoreCase)).Any(),
                        "X509CertificateName not found for software-product-cert-common-name");

        }
    }
}
