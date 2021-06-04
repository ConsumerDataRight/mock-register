using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NSubstitute;
using CDR.Register.IdentityServer.Services;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Net.Http;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using CDR.Register.IdentityServer.Caching;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Models;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class JwkServiceUnitTests
    {
        private ILogger<JwkService> _logger = Substitute.For<ILogger<JwkService>>();
        private IMediator _mediator = Substitute.For<IMediator>();
        private HttpClient _httpClient;

        private readonly string _jwksUrl = "https://localhost/";
        private JwkService _jwkService;
                

        [Fact]        
        public async Task StatusNotFound_Should_Raise_NotificationMessage801()
        {
            //Arrange
            _httpClient = JwkServiceMockHttpClients.HttpClientWithResponseMessageNotFound();
            _jwkService = new JwkService(_logger, _httpClient, _mediator);

            //Act
            await _jwkService.GetJwksAsync(_jwksUrl);

            //Assert            
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "801"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "801"));
        }

        [Fact]
        public async Task StatusBadRequest_Should_Raise_NotificationMessage803()
        {
            //Arrange
            _httpClient = JwkServiceMockHttpClients.HttpClientWithResponseMessageBadRequest();
            _jwkService = new JwkService(_logger, _httpClient, _mediator);

            //Act
            //Ignore exception here, only check if _mediator is called
            try
            {
                await _jwkService.GetJwksAsync(_jwksUrl);
            }
            catch { }

            //Assert            
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "803"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "803"));
        }

        [Fact]
        public async Task StatusOK_Content_No_JWKS_Should_Raise_NotificationMessage802()
        {
            //Arrange
            _httpClient = JwkServiceMockHttpClients.HttpClientWithResponseMessageOK_No_JWKS_Content();
            _jwkService = new JwkService(_logger, _httpClient, _mediator);

            //Act
            await _jwkService.GetJwksAsync(_jwksUrl);

            //Assert            
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "802"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "802"));
        }

        [Fact]
        public async Task StatusOK_Content_Contains_JWKS_Should_Raise_NotificationMessage0()
        {
            //Arrange
            _httpClient = JwkServiceMockHttpClients.HttpClientWithResponseMessageOK_Contains_JWKS_Content();
            _jwkService = new JwkService(_logger, _httpClient, _mediator);

            //Act
            await _jwkService.GetJwksAsync(_jwksUrl);

            //Assert            
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
        }
    }
}
