using CDR.Register.IdentityServer.Caching;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Models;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using FluentAssertions;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static CDR.Register.IdentityServer.Tests.UnitTests.Setup.CdrConstants;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class TokenEndpointUnitTests : IClassFixture<TokenEndpointWebApplicationFactory<Startup>>
    {
        private readonly string _tokenEndpoint;
        private readonly TokenEndpointWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;
        private readonly IJwkService _mockJwkService = Substitute.For<IJwkService>();
        private readonly IClientStore _mockClientStore = Substitute.For<IClientStore>();
        private readonly IPersistedGrantStore _mockPersistedGrantStore = Substitute.For<IPersistedGrantStore>();
        private readonly ISecretParser _mockSecretParser = Substitute.For<ISecretParser>();
        private readonly ISecretValidator _mockSecretValidator = Substitute.For<ISecretValidator>();
        private readonly IMediator _mockMediator = Substitute.For<IMediator>();
        private readonly ILruCache _lru = Substitute.For<ILruCache>();

        public TokenEndpointUnitTests(TokenEndpointWebApplicationFactory<Startup> factory)
        {
            _mockClientStore.FindClientByIdAsync(Arg.Any<string>()).Returns(Task.FromResult(JwkMock.MockClient));

            var parsedSecret = JwkMock.GetValidMockParsedSecret();
            _mockSecretParser.ParseAsync(default).ReturnsForAnyArgs(parsedSecret);
            _lru.AddCache(Arg.Any<string>(), Arg.Any<DateTime>()).Returns(true);
            _factory = factory;
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Add(new ServiceDescriptor(typeof(IJwkService), _mockJwkService));
                    services.Add(new ServiceDescriptor(typeof(IClientStore), _mockClientStore));
                    services.Add(new ServiceDescriptor(typeof(IPersistedGrantStore), _mockPersistedGrantStore));
                    services.Add(new ServiceDescriptor(typeof(ISecretParser), _mockSecretParser));
                    services.Add(new ServiceDescriptor(typeof(ISecretValidator), _mockSecretValidator));
                    services.Add(new ServiceDescriptor(typeof(IMediator), _mockMediator));
                    services.Add(new ServiceDescriptor(typeof(ILruCache), _lru));
                });
            }).CreateClient();

            _tokenEndpoint = $"{_client.BaseAddress}connect/token";
            _client.DefaultRequestHeaders.Add(CustomHeaders.ClientCertClientNameHeaderKey, JwkMock.ValidBrandCertificateName);
            _client.DefaultRequestHeaders.Add(CustomHeaders.ClientCertThumbprintHeaderKey, JwkMock.ValidCertificateThumbprint);
        }

        [Fact]
        public async Task Validation_Pass_Should_Raise_NotificationMessage0()
        {
            //Arrange            
            _mockSecretValidator.ValidateAsync(default, default).ReturnsForAnyArgs(JwkMock.GetMockValidationResultSuccess());

            var nameValueCollection = new Dictionary<string, string>
                {
                    { TokenRequest.GrantType, JwkMock.GrantType },
                    { TokenRequest.ClientId, JwkMock.ValidClientId },
                    { TokenRequest.ClientAssertionType, ClientAssertionTypes.JwtBearer },
                    { TokenRequest.ClientAssertion, JwkMock.CreateValidMockClientAssertion() },
                    { TokenRequest.Scope, JwkMock.BankScope },
                };
            var formContent = new FormUrlEncodedContent(nameValueCollection);

            //Action
            var response = await _client.PostAsync(_tokenEndpoint, formContent).ConfigureAwait(false);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            await _mockMediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0" && m.Type == "LoggingEventSink_Authentication"));
            await _mockMediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0" && m.Type == "LoggingEventSink_Token"));
            await _mockMediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code == "1000"));
        }

        [Fact]
        public async Task Validation_Fail_Should_Raise_NotificationMessage1000()
        {
            //Arrange            
            _mockSecretValidator.ValidateAsync(default, default).ReturnsForAnyArgs(JwkMock.GetMockValidationResultFailed());

            var nameValueCollection = new Dictionary<string, string>
                {
                    { TokenRequest.GrantType, JwkMock.GrantType },
                    { TokenRequest.ClientId, JwkMock.ValidClientId },
                    { TokenRequest.ClientAssertionType, ClientAssertionTypes.JwtBearer },
                    { TokenRequest.ClientAssertion, JwkMock.CreateValidMockClientAssertion() },
                    { TokenRequest.Scope, JwkMock.BankScope },
                };
            var formContent = new FormUrlEncodedContent(nameValueCollection);

            //Action
            var response = await _client.PostAsync(_tokenEndpoint, formContent).ConfigureAwait(false);

            //Assert
            await _mockMediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "1000" && m.Type.StartsWith("LoggingEventSink_")));
            await _mockMediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code == "0" && m.Type.StartsWith("LoggingEventSink_")));
        }
    }
}
