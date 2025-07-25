﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Enums;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CDR.Register.Repository
{
    public class RegisterAdminRepository : IRegisterAdminRepository
    {
        private readonly RegisterDatabaseContext _registerDatabaseContext;
        private readonly IMapper _mapper;
        private readonly IRepositoryMapper _reporsitoryMapper;
        private readonly ILogger<RegisterAdminRepository> _logger;

        public RegisterAdminRepository(RegisterDatabaseContext registerDatabaseContext, IMapper mapper, IRepositoryMapper repositoryMapper, ILogger<RegisterAdminRepository> logger)
        {
            this._registerDatabaseContext = registerDatabaseContext;
            this._mapper = mapper;
            this._reporsitoryMapper = repositoryMapper;
            this._logger = logger;
        }

        public async Task<DataHolderBrand> GetDataHolderBrandAsync(Guid brandId)
        {
            var dataHolderBrandEntity = await this._registerDatabaseContext.Brands.AsNoTracking()
                .Include(b => b.Participation.LegalEntity)
                .Where(b => b.BrandId == brandId).FirstOrDefaultAsync();
            if (dataHolderBrandEntity == null)
            {
                return null;
            }

            var brand = new DataHolderBrand
            {
                LogoUri = dataHolderBrandEntity.LogoUri,
                BrandId = dataHolderBrandEntity.BrandId,
                BrandName = dataHolderBrandEntity.BrandName,
                BrandStatus = dataHolderBrandEntity.BrandStatusId.ToString(),
            };
            if (dataHolderBrandEntity.Participation.ParticipationTypeId == ParticipationTypes.Dh)
            {
                brand.DataHolder = new DataHolder
                {
                    DataHolderId = dataHolderBrandEntity.ParticipationId,
                    Industry = dataHolderBrandEntity.Participation?.IndustryId.ToString(),
                    Status = dataHolderBrandEntity.Participation.StatusId.ToString(),
                    LegalEntity = this._mapper.Map<DataHolderLegalEntity>(dataHolderBrandEntity.Participation.LegalEntity),
                };
            }

            return brand;
        }

        public async Task<Participation> GetDataRecipientIdAsync(Guid legalEntityId)
        {
            return await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Industry)
                .Include(p => p.LegalEntity)
                .Include(p => p.Brands)
                    .ThenInclude(brand => brand.BrandStatus)
                .Include(p => p.Brands)
                    .ThenInclude(brand => brand.SoftwareProducts)
                    .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypes.Dr && p.LegalEntityId == legalEntityId)
                .SingleOrDefaultAsync();
        }

        public async Task<BusinessRuleError> AddOrUpdateDataRecipient(DataRecipient dataRecipient)
        {
            var existingDataRecipient = await this.GetDataRecipientIdAsync(dataRecipient.LegalEntity.LegalEntityId);

            using var transaction = await this._registerDatabaseContext.Database.BeginTransactionAsync();

            LegalEntity legalEntity = this._reporsitoryMapper.Map(dataRecipient.LegalEntity);

            var existingLegalEntity = await this._registerDatabaseContext.LegalEntities.AsNoTracking().SingleOrDefaultAsync(x => x.LegalEntityId == dataRecipient.LegalEntity.LegalEntityId);

            if (existingDataRecipient != null && existingLegalEntity != null)
            {
                this._registerDatabaseContext.LegalEntities.Update(legalEntity);
            }

            if (existingDataRecipient == null && existingLegalEntity == null)
            {
                await this._registerDatabaseContext.LegalEntities.AddAsync(legalEntity);
                this._logger.LogInformation("New LegalEntity of id:{LegalEntityId} name:{LegalEntityName} getting added to the repository.", legalEntity.LegalEntityId, legalEntity.LegalEntityName);
            }

            var error = await dataRecipient.AddOrUpdateDataRecipientLegalEntity(legalEntity, this._registerDatabaseContext, this._reporsitoryMapper, this._logger);
            if (error == null)
            {
                await this._registerDatabaseContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return error;
        }

        public async Task<bool> SaveDataHolderBrand(Guid legalEntityId, DataHolderBrand dataHolderBrand)
        {
            if (dataHolderBrand == null)
            {
                return false;
            }

            (_, var savedParticipation) = await this.SaveDataHolderLegalEntity(dataHolderBrand.DataHolder);

            // Save DH Brand
            var dhBrandToSave = this._mapper.Map<Entities.Brand>(dataHolderBrand);
            var existingBrand = await this._registerDatabaseContext.Brands
                .Include(b => b.Participation)
                .Include(b => b.AuthDetails)
                .Include(b => b.Endpoint)
                .Where(brand => brand.BrandId == dataHolderBrand.BrandId).FirstOrDefaultAsync();
            if (existingBrand == null)
            {
                dhBrandToSave.LastUpdated = DateTime.UtcNow;
                savedParticipation.Brands.Add(dhBrandToSave);
            }
            else
            {
                dhBrandToSave = this._mapper.Map(dataHolderBrand, existingBrand);
                dhBrandToSave.LastUpdated = DateTime.UtcNow;
            }

            // Add Endpoints data
            if (dataHolderBrand.DataHolderBrandServiceEndpoint != null)
            {
                var existingEndpoint = existingBrand?.Endpoint;
                if (existingEndpoint == null)
                {
                    var endpointToSave = this._mapper.Map<Endpoint>(dataHolderBrand.DataHolderBrandServiceEndpoint);
                    endpointToSave.Brand = dhBrandToSave;
                    this._registerDatabaseContext.Endpoints.Add(endpointToSave);
                }
                else
                {
                    this._mapper.Map(dataHolderBrand.DataHolderBrandServiceEndpoint, existingEndpoint);
                }
            }

            // Add AuthDetail data
            if (dataHolderBrand.DataHolderAuthentications.Any())
            {
                var existingAuthDetail = existingBrand?.AuthDetails?.FirstOrDefault();
                if (existingAuthDetail == null)
                {
                    var authDetailToSave = this._mapper.Map<AuthDetail>(dataHolderBrand.DataHolderAuthentications[0]);
                    authDetailToSave.Brand = dhBrandToSave;
                    this._registerDatabaseContext.AuthDetails.Add(authDetailToSave);
                }
                else
                {
                    this._mapper.Map(dataHolderBrand.DataHolderAuthentications[0], existingAuthDetail);
                }
            }

            await this._registerDatabaseContext.SaveChangesAsync();
            return true;
        }

        private async Task<(LegalEntity LegalEntity, Participation Participation)> SaveDataHolderLegalEntity(DataHolder dataHolder)
        {
            var participationToSave = this._mapper.Map<Participation>(dataHolder);

            // Check if the entity exists. If so, update the existing values
            var existingLegalEntity = await this._registerDatabaseContext.LegalEntities
                .Include(le => le.Participations)
                .Where(le => le.LegalEntityId == dataHolder.LegalEntity.LegalEntityId)
                .FirstOrDefaultAsync();
            var legalEntityToSave = this._mapper.Map<LegalEntity>(dataHolder.LegalEntity);
            if (existingLegalEntity == null)
            {
                participationToSave.LegalEntity = legalEntityToSave;
                this._registerDatabaseContext.LegalEntities.Add(legalEntityToSave);
                this._registerDatabaseContext.Participations.Add(participationToSave);

                return (legalEntityToSave, participationToSave);
            }

            // Only update the DH Brand related information.
            this._mapper.Map(dataHolder.LegalEntity, existingLegalEntity);

            // Update participation details
            var existingDhParticipation = existingLegalEntity.Participations.FirstOrDefault(p =>
                p.ParticipationTypeId == ParticipationTypes.Dh && p.IndustryId == participationToSave.IndustryId);
            if (existingDhParticipation == null)
            {
                participationToSave.LegalEntity = existingLegalEntity;
                this._registerDatabaseContext.Participations.Add(participationToSave);
            }
            else
            {
                participationToSave = this._mapper.Map(dataHolder, existingDhParticipation);
            }

            return (existingLegalEntity, participationToSave);
        }
    }
}
