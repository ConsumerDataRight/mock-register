//using CDR.Register.IdentityServer.Caching;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Models;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class CdrSecretValidatorUnitTests
    {
        private readonly IJwkService _jwkService = Substitute.For<IJwkService>();
        private readonly ILogger<CdrSecretValidator> _logger = Substitute.For<ILogger<CdrSecretValidator>>();
        private readonly IdentityServerOptions _identityServerOptions = Substitute.For<IdentityServerOptions>();
        private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        //private readonly ILruCache _lru = Substitute.For<ILruCache>();
        private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        private readonly string _issuerBaseUri = "http://localhost-api-unittest";
        private readonly string _issuerPath = "/cts-register";

        private CdrSecretValidator _cdrSecretValidator;

        public CdrSecretValidatorUnitTests()
        {
            _identityServerOptions.IssuerUri = _issuerBaseUri;
            _configuration["Conformance:IssuerBaseUri"].Returns(_issuerBaseUri);
            _configuration["Conformance:IssuerPath"].Returns(_issuerPath);

            _cdrSecretValidator = new CdrSecretValidator(
                _identityServerOptions,
                _jwkService,
                _configuration,
                _logger,
                _mediator);
        }

        [Fact]
        public async Task ParsedSecret_Null_Should_Raise_NotificationMessage604()
        {
            //Arrange

            //Act
            await _cdrSecretValidator.ValidateAsync(null, null);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "604"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "604"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("NotCdrSecret")]
        public async Task ParsedSecret_Type_Invalid_Should_Raise_NotificationMessage605(string parsedSecretType)
        {
            //Arrange
            ParsedSecret parsedSecret = new ParsedSecret
            {
                Id = Guid.NewGuid().ToString(),
                Type = parsedSecretType,
            };

            //Act
            await _cdrSecretValidator.ValidateAsync(null, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "605"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "605"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("MockCredential")]
        public async Task ParsedSecret_Credential_Invalid_Should_Raise_NotificationMessage606(string parsedSecretCredential)
        {
            //Arrange
            ParsedSecret parsedSecret = new ParsedSecret
            {
                Id = Guid.NewGuid().ToString(),
                Type = "CdrSecret",
                Credential = parsedSecretCredential,
            };

            //Act
            await _cdrSecretValidator.ValidateAsync(null, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "606"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "606"));
        }

        //[Fact]
        //public async Task Cannot_Parse_Secret_Should_Raise_NotificationMessage607()
        //{
        //    //Arrange            
        //    var secrets = Substitute.For<IEnumerable<Secret>>();
        //    var parsedSecret = JwkMock.GetMockParsedSecret("mock id", "mock token");

        //    secrets.When(x => x.GetJsonWebKeysAsync(_jwkService)).Do(x => { throw new Exception(); });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "607"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "607"));
        //}

        //[Fact]
        //public async Task Cannot_Find_Match_Key_Should_Raise_NotificationMessage608()
        //{
        //    //Arrange
        //    var secrets = JwkMock.GetMockSecrets("mock type not JWKSURL");
        //    var parsedSecret = JwkMock.GetMockParsedSecret("mock id", "mock token");

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "608"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "608"));
        //}

        //[Theory]
        //[InlineData(null)]
        //[InlineData("")]
        //public async Task Missing_Jti_Should_Raise_NotificationMessage709(string jti)
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, validClientId),
        //    };
        //    if (jti != null)
        //    {
        //        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));
        //    }

        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken);

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "709"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "709"));
        //}

        //[Fact(Skip = "Cts doesn't validate jti unique")]
        //public async Task Duplicated_Jti_Should_Raise_NotificationMessage710()
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var aud = _identityServerOptions.IssuerUri + "/connect/token";
        //    var jti = "DuplicatedJti";
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, validClientId),
        //        new Claim(JwtRegisteredClaimNames.Jti, jti),
        //    };

        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken);

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Setup duplicated records
        //    var tokenIdentifier = $"{aud}:{jti}";

        //    //_lru.AddCache(Arg.Any<string>(), Arg.Any<DateTime>()).Returns(false);

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "710"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "710"));

        //    ////Reset the LRU Cache to return true for adding to cache
        //    //_lru.AddCache(Arg.Any<string>(), Arg.Any<DateTime>()).Returns(true);
        //}

        //[Fact]
        //public async Task Subject_Not_Match_Issuer_Should_Raise_NotificationMessage712()
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, "Not matched issuer"),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //    };

        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken);

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "712"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "712"));
        //}

        //[Fact]
        //public async Task Jwt_Token_Invalid_Exception_Should_Raise_NotificationMessage711()
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, validClientId),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //    };

        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, "not valid jwt token");

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "711"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "711"));
        //}

        [Fact]
        public async Task Invalid_CertificateThumbprint_Should_Raise_NotificationMessage609()
        {
            //Arrange
            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();
            var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken, "CdrSecret", StringValues.Empty);

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "609"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "609"));
        }

        //[Fact]
        //public async Task Invalid_CertificateCommonName_Should_Raise_NotificationMessage610()
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, validClientId),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //    };
        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken, "CdrSecret", new StringValues("mock CertificateThumbprint"), StringValues.Empty);

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "610"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "610"));
        //}

        [Fact]
        public async Task Not_Match_CertificateCommonName_Should_Raise_NotificationMessage611()
        {
            //Arrange
            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();
            var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken);

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "611"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "611"));
        }

        //[Fact]
        //public async Task Valid_Token_Should_Raise_NotificationMessage0()
        //{
        //    //Arrange
        //    string validClientId = Guid.NewGuid().ToString();
        //    var conformanceId = Guid.NewGuid().ToString();

        //    _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(Arg.Any<string>(), out var _).Returns(x =>
        //    {
        //        x[1] = new StringValues(conformanceId);
        //        return true;
        //    });

        //    var audience = $"{_issuerBaseUri}/{conformanceId}{_issuerPath}/connect/token";

        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, validClientId),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),           
        //    };

        //    var securityKey = JwkMock.GetX509SecurityKey();
        //    var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey, audience);

        //    var secrets = JwkMock.GetMockSecrets();
        //    var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken, "CdrSecret", new StringValues(JwkMock.ValidCertificateThumbprint), new StringValues(JwkMock.ValidBrandCertificateName));

        //    JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
        //    _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

        //    //Act
        //    await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

        //    //Assert
        //    await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
        //    await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
        //}

        [Fact]
        public async Task Valid_Brand_Cert_Should_Raise_NotificationMessage0()
        {
            //Arrange
            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  _identityServerOptions.IssuerUri + "/connect/token")
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();
            var parsedSecret = JwkMock.GetMockParsedSecret(
                validClientId, jwtToken, "CdrSecret", new StringValues(JwkMock.ValidCertificateThumbprint), new StringValues(JwkMock.ValidBrandCertificateName));

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
        }

        [Fact]
        public async Task Valid_Software_Product_Cert_Should_Raise_NotificationMessage0()
        {
            //Arrange
            string validClientId = "TestClient";//Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(JwtRegisteredClaimNames.Aud,  _identityServerOptions.IssuerUri + "/connect/token")
                new Claim(JwtRegisteredClaimNames.Aud,  "https://localhost:7002/connect/token")
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();
            var parsedSecret = JwkMock.GetMockParsedSecret(
                validClientId, jwtToken, "CdrSecret", new StringValues(JwkMock.ValidCertificateThumbprint), new StringValues(JwkMock.ValidSoftwareProductCertificateName));

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "0"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "0"));
        }

        [Fact]
        public async Task Multiple_CertificateThumbprint_Should_Raise_NotificationMessage609()
        {
            //Arrange
            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  _identityServerOptions.IssuerUri + "/connect/token")
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();

            string[] certThumbprints = new string[2] { new StringValues(JwkMock.ValidCertificateThumbprint), "mock CertificateThumbprint" };
            var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken, "CdrSecret",
                certThumbprints, new StringValues(JwkMock.ValidBrandCertificateName));

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "609"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "609"));
        }

        [Fact]
        public async Task Multiple_CertificateCommonName_Should_Raise_NotificationMessage610()
        {
            //Arrange
            string validClientId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, validClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Aud,  _identityServerOptions.IssuerUri + "/connect/token")
            };

            var securityKey = JwkMock.GetX509SecurityKey();
            var jwtToken = JwkMock.GetJwtToken(validClientId, claims, securityKey);

            var secrets = JwkMock.GetMockSecrets();

            string[] certNames = new string[2] { new StringValues(JwkMock.ValidBrandCertificateName), "mock CertificateCommonName" };
            var parsedSecret = JwkMock.GetMockParsedSecret(validClientId, jwtToken, "CdrSecret",
                new StringValues(JwkMock.ValidCertificateThumbprint), new StringValues(certNames));

            JsonWebKey jsonWebKey = JsonWebKeyConverter.ConvertFromX509SecurityKey(securityKey);
            _jwkService.GetJwksAsync(default).ReturnsForAnyArgs(new List<JsonWebKey>() { jsonWebKey });

            //Act
            await _cdrSecretValidator.ValidateAsync(secrets, parsedSecret);

            //Assert
            await _mediator.Received(1).Publish(Arg.Is<NotificationMessage>(m => m.Code == "610"));
            await _mediator.DidNotReceive().Publish(Arg.Is<NotificationMessage>(m => m.Code != "610"));
        }
    }
}
