using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Discovery.API.Business;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Repository.Specifications;
using Google.Protobuf.Compiler;
using NSubstitute;

namespace CDR.Register.Discovery.API.UnitTests
{
    public class DiscoveryServiceTests
    {
        private readonly IRegisterDiscoveryRepository _repository = Substitute.For<IRegisterDiscoveryRepository>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        [Theory]
        [InlineData(2, typeof(BrandSpecifications.ExcludeNblIndustry))]
        [InlineData(3, typeof(BrandSpecifications.AllIndustries))]
        public async Task GetDataHolderBrands_UsesCorrectSpecification(int version, Type expectedType)
        {
            // Arrange
            Type specificationType = null;
            _ = await this._repository.GetDataHolderBrands(Industry.ALL, Arg.Do<IBrandSpecification>(x => specificationType = x.GetType()), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>());
            var service = new DiscoveryService(this._repository, this._mapper);

            // Act
            _ = await service.GetDataHolderBrands(Industry.ALL, DateTime.UtcNow, 1, 1, version);

            // Assert
            Assert.Equal(expectedType, specificationType);
        }

        [Fact]
        public async Task GetDataHolderBrands_ThrowsExceptionForUnsupportedVersion()
        {
            // Arrange
            var service = new DiscoveryService(this._repository, this._mapper);

            // Act
            var action = async () => await service.GetDataHolderBrands(Industry.ALL, DateTime.UtcNow, 1, 1, 1);

            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(action);
        }
    }
}
