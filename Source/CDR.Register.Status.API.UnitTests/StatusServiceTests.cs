using System;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Repository.Specifications;
using CDR.Register.Status.API.Business;
using NSubstitute;

namespace CDR.Register.Status.API.UnitTests
{
    public class StatusServiceTests
    {
        private readonly IRegisterStatusRepository _repository = Substitute.For<IRegisterStatusRepository>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        [Theory]
        [InlineData(1, typeof(ParticipationsSpecifications.ExcludeNblIndustry))]
        [InlineData(2, typeof(ParticipationsSpecifications.AllIndustries))]
        public async Task GetDataHolderStatuses_UsesCorrectSpecification(int version, Type expectedType)
        {
            // Arrange
            Type specificationType = null;
            _ = await this._repository.GetDataHolderStatuses(Industry.ALL, Arg.Do<IParticipationSpecification>(x => specificationType = x.GetType()));
            var service = new StatusService(this._repository, this._mapper);

            // Act
            _ = await service.GetDataHolderStatuses(Industry.ALL, version);

            // Assert
            Assert.Equal(expectedType, specificationType);
        }

        [Fact]
        public async Task GetDataHolderStatuses_ThrowsExceptionForUnsupportedVersion()
        {
            // Arrange
            var service = new StatusService(this._repository, this._mapper);

            // Act
            var action = async () => await service.GetDataHolderStatuses(Industry.ALL, 5);

            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(action);
        }

        [Theory]
        [InlineData(2, typeof(ParticipationsSpecifications.ExcludeNblIndustry))]
        [InlineData(3, typeof(ParticipationsSpecifications.AllIndustries))]
        public async Task GetDataRecipientsStatuses_UsesCorrectSpecification(int version, Type expectedType)
        {
            // Arrange
            Type specificationType = null;
            _ = await this._repository.GetDataRecipientStatuses(Industry.ALL, Arg.Do<IParticipationSpecification>(x => specificationType = x.GetType()));
            var service = new StatusService(this._repository, this._mapper);

            // Act
            _ = await service.GetDataRecipientStatuses(Industry.ALL, version);

            // Assert
            Assert.Equal(expectedType, specificationType);
        }

        [Fact]
        public async Task GetDataRecipientsStatuses_ThrowsExceptionForUnsupportedVersion()
        {
            // Arrange
            var service = new StatusService(this._repository, this._mapper);

            // Act
            var action = async () => await service.GetDataRecipientStatuses(Industry.ALL, 1);

            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(action);
        }

        [Theory]
        [InlineData(2, typeof(ParticipationsSpecifications.ExcludeNblIndustry))]
        [InlineData(3, typeof(ParticipationsSpecifications.AllIndustries))]
        public async Task GetAdrSoftwareProductStatuses_UsesCorrectSpecification(int version, Type expectedType)
        {
            // Arrange
            Type specificationType = null;
            _ = await this._repository.GetSoftwareProductStatuses(Industry.ALL, Arg.Do<IParticipationSpecification>(x => specificationType = x.GetType()));
            var service = new StatusService(this._repository, this._mapper);

            // Act
            _ = await service.GetSoftwareProductStatuses(Industry.ALL, version);

            // Assert
            Assert.Equal(expectedType, specificationType);
        }

        [Fact]
        public async Task GetAdrSoftwareProductStatuses_ThrowsExceptionForUnsupportedVersion()
        {
            // Arrange
            var service = new StatusService(this._repository, this._mapper);

            // Act
            var action = async () => await service.GetSoftwareProductStatuses(Industry.ALL, 1);

            // Assert
            await Assert.ThrowsAsync<NotImplementedException>(action);
        }
    }
}
