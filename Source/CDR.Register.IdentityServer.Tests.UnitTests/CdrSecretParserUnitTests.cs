using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Models;
//using CDR.Register.IdentityServer.Caching;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using IdentityModel;
using IdentityServer4.Configuration;
using MediatR;
//using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Xunit;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class CdrSecretParserUnitTests
    {
        private readonly IdentityServerOptions _options = Substitute.For<IdentityServerOptions>();
        private readonly ILogger<CdrSecretParser> _logger = Substitute.For<ILogger<CdrSecretParser>>();
        private readonly IMediator _mediator = Substitute.For<IMediator>();

        private HttpContext _mockHttpContext = Substitute.For<HttpContext>();
        private HttpRequest _mockHttpRequest = Substitute.For<HttpRequest>();

        private CdrSecretParser _cdrSecretParser;

        public CdrSecretParserUnitTests()
        {
            _cdrSecretParser = new CdrSecretParser(_options, _logger, _mediator);
        }

        [Fact]
        public async Task Missing_Header_X_TlsClientCertThumbprint_Should_Raise_NotificationMessage600()
        {
            //arrange
            var headers = JwkMock.GetMockHeader(null, "mock X-TlsClientCertCN", null);
            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "600"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "600"));
        }

        [Fact]
        public async Task Missing_Header_X_TlsClientCertCN_Should_Raise_NotificationMessage603()
        {
            //arrange
            var headers = JwkMock.GetMockHeader("mock X-TlsClientCertThumbprint", null, null);
            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "603"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "603"));
        }

        [Fact]
        public async Task Missing_Header_Content_Type_Should_Raise_NotificationMessage902()
        {
            //arrange
            var headers = JwkMock.GetMockHeader("mock X-TlsClientCertThumbprint", "mock X-TlsClientCertCN", null);
            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "902"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "902"));
        }

        [Fact]
        public async Task Null_Body_Should_Raise_NotificationMessage902()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();
            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>());
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "902"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "902"));
        }

        [Fact]
        public async Task ClientAssertion_Exceeds_Maximum_Length_Should_Raise_NotificationMessage705()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();

            //string validClientId = Guid.NewGuid().ToString();
            string validClientId = "TestClient";
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost:7000/idp/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>
            {
                {OidcConstants.TokenRequest.ClientId, new StringValues(validClientId) },
                { OidcConstants.TokenRequest.ClientAssertionType, CdrConstants.ClientAssertionTypes.JwtBearer},
                { OidcConstants.TokenRequest.ClientAssertion, jwtToken },
            });

            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));
            _options.InputLengthRestrictions.Jwt = 1;

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "705"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "705"));
        }

        [Fact]
        public async Task ClientAssertion_Subclaim_Missing_Should_Raise_NotificationMessage706()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();

            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                //missing sub in claims to test
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost:7000/idp/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>
            {
                {OidcConstants.TokenRequest.ClientId, new StringValues(validClientId) },
                { OidcConstants.TokenRequest.ClientAssertionType, CdrConstants.ClientAssertionTypes.JwtBearer},
                { OidcConstants.TokenRequest.ClientAssertion, jwtToken },
            });

            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "706"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "706"));
        }

        [Fact]
        public async Task ClientAssertion_Subclaim_Exceeds_Max_Length_Should_Raise_NotificationMessage707()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();

            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost:7000/idp/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>
            {
                { OidcConstants.TokenRequest.ClientId, new StringValues(validClientId) },
                { OidcConstants.TokenRequest.ClientAssertionType, CdrConstants.ClientAssertionTypes.JwtBearer},
                { OidcConstants.TokenRequest.ClientAssertion, jwtToken },
            });

            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));
            _options.InputLengthRestrictions.ClientId = 1;

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "707"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "707"));
        }

        [Fact]
        public async Task ClientAssertion_Subclaim_Not_Match_Body_ClientId_Should_Raise_NotificationMessage708()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();

            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost:7000/idp/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>
            {
                {OidcConstants.TokenRequest.ClientId, new StringValues("not matched client id") },
                { OidcConstants.TokenRequest.ClientAssertionType, CdrConstants.ClientAssertionTypes.JwtBearer},
                { OidcConstants.TokenRequest.ClientAssertion, jwtToken },
            });

            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            //----await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "708"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "708"));
        }

        [Fact]
        public async Task Valid_Should_Not_Raise_NotificationMessage0()
        {
            //arrange
            var headers = JwkMock.GetMockHeader();

            string validClientId = "TestClient"; //Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  "http://localhost:5000/connect/token")
            };
            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            IFormCollection body = new FormCollection(new Dictionary<string, StringValues>
            {
                {OidcConstants.TokenRequest.ClientId, new StringValues(validClientId) },
                { OidcConstants.TokenRequest.ClientAssertionType, CdrConstants.ClientAssertionTypes.JwtBearer},
                { OidcConstants.TokenRequest.ClientAssertion, jwtToken },
            });

            _mockHttpRequest.Headers.Returns(headers);
            _mockHttpContext.Request.Returns(_mockHttpRequest);
            _mockHttpRequest.HasFormContentType.Returns(true);
            _mockHttpContext.Request.ReadFormAsync().Returns(Task.FromResult(body));

            //act
            await _cdrSecretParser.ParseAsync(_mockHttpContext);

            //assert
            //Note: CdrSecretParser and CdrSecretValidator only send ONE MTLS pass record at the end of CdrSecretValidator            
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => !string.IsNullOrEmpty(m.Code)));
        }
    }
}
