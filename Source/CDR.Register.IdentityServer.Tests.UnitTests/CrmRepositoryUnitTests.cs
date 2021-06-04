using AutoMapper;
using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Repositories;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CDR.Register.IdentityServer.Tests.UnitTests
{
    public class CrmRepositoryUnitTests
    {
        [Fact]
        public async Task GetDataRecipientBrandFromCrm_UnitTest()
        {
            // Arrange
            ILogger<CrmClientQueryRepository> mockLogger = Substitute.For<ILogger<CrmClientQueryRepository>>();
            HttpClient mockHttpClient = CrmApiMockHttpClient.HttpClient_DataRecipientBrand();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfig.CreateMapper();

            string mockBrandId = Guid.NewGuid().ToString();

            // Action
            var crmRepo = new CrmClientQueryRepository(mockLogger, mockHttpClient, mapper, ConfigEntryMock.GetMockConfigEntry());
            var result = await crmRepo.GetCrmClientAsync(mockBrandId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockBrandId, result.Id);
            Assert.Equal("CTS FINTECH Brand :  001208", result.Name);
            Assert.Equal(@"https://localhost/cdr-register/v1/jwks", result.JwksUri);
            Assert.NotNull(result.X509Certificates);
            Assert.Single(result.X509Certificates);
            Assert.Equal("brand-cert-common-name", result.X509Certificates[0].CommonName);
            Assert.Equal("87af1214f736051a9e14baf0f205862691cce164", result.X509Certificates[0].Thumbprint);
        }

        [Fact]
        public async Task GetDataRecipientProductsFromCrm_UnitTest()
        {
            // Arrange
            ILogger<CrmClientQueryRepository> mockLogger = Substitute.For<ILogger<CrmClientQueryRepository>>();
            HttpClient mockHttpClient = CrmApiMockHttpClient.HttpClient_DataNoBrandOnlyProduct();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfig.CreateMapper();

            string mockProductId = Guid.NewGuid().ToString();

            // Action
            var crmRepo = new CrmClientQueryRepository(mockLogger, mockHttpClient, mapper, ConfigEntryMock.GetMockConfigEntry());
            var result = await crmRepo.GetCrmClientAsync(mockProductId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockProductId, result.Id);
            Assert.Equal("https://api.dev.cdr.gov.au/cdr-register/v1/jwks", result.JwksUri);
            Assert.Equal("Principal Product in Collections Arrangement", result.Name);
        }

        [Fact]
        public async Task GetDataRecipientProductsInactiveProviderFromCrm_UnitTest()
        {
            // Arrange
            ILogger<CrmClientQueryRepository> mockLogger = Substitute.For<ILogger<CrmClientQueryRepository>>();
            HttpClient mockHttpClient = CrmApiMockHttpClient.HttpClient_DataNoBrandOnlyProduct(true);

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperConfig>());
            IMapper mapper = mapperConfig.CreateMapper();

            string mockProductId = Guid.NewGuid().ToString();

            // Action
            var crmRepo = new CrmClientQueryRepository(mockLogger, mockHttpClient, mapper, ConfigEntryMock.GetMockConfigEntry());
            var result = await crmRepo.GetCrmClientAsync(mockProductId);

            // Assert
            Assert.Null(result);
        }
    }
}
