﻿using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDR.Register.Repository
{
    public class RegisterDiscoveryRepository : IRegisterDiscoveryRepository
    {
        private readonly RegisterDatabaseContext _registerDatabaseContext;
        private readonly IMapper _mapper;

        public RegisterDiscoveryRepository(RegisterDatabaseContext registerDatabaseContext, IMapper mapper)
        {
            this._registerDatabaseContext = registerDatabaseContext;
            this._mapper = mapper;
        }

        public async Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsyncXV1(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            (List<Entities.Brand> allBrands, int totalRecords) = await ProcessGetDataHolderBrands(industry, updatedSince, page, pageSize);

            return new Page<DataHolderBrand[]>()
            {
                Data = _mapper.Map<DataHolderBrand[]>(allBrands),
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<Page<DataHolderBrandV2[]>> GetDataHolderBrandsAsyncXV2(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            (List<Entities.Brand> allBrands, int totalRecords) = await ProcessGetDataHolderBrands(industry, updatedSince, page, pageSize);

            return new Page<DataHolderBrandV2[]>()
            {
                Data = _mapper.Map<DataHolderBrandV2[]>(allBrands),
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        protected async Task<(List<Entities.Brand>, int)> ProcessGetDataHolderBrands(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var allBrandsQuery = this._registerDatabaseContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Include(brand => brand.Participation.Status)
                .Where(brand => brand.Participation.ParticipationTypeId == ParticipationTypes.Dh)
                .Where(brand => brand.Participation.LegalEntity.LegalEntityStatusId == LegalEntityStatusType.Active)
                .Where(brand => brand.Participation.StatusId == ParticipationStatusType.Active)
                .Where(brand => brand.BrandStatusId == BrandStatusType.Active);

            if (industry != Industry.ALL)
            {
                allBrandsQuery = allBrandsQuery.Where(brand => brand.Participation.Industry.IndustryTypeId == industry);
            }

            // Add the updated since filter
            if (updatedSince.HasValue)
            {
                allBrandsQuery = allBrandsQuery.Where(brand => brand.LastUpdated > updatedSince.Value);
            }

            var totalRecords = await allBrandsQuery.CountAsync();

            // Apply ordering and pagination
            var allBrands = await allBrandsQuery
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (allBrands, totalRecords);
        }

        public async Task<DataRecipient[]> GetDataRecipientsAsyncXV1(Industry industry)
        {
            // UNKNOWN industry type is parsed to NOT use industry filtering.
            List<Participation> allParticipants = await ProcessGetDataRecipients(industry);

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipants.ForEach(p =>
            {
                p.Industry = new IndustryType() { IndustryTypeId = industry, IndustryTypeCode = industry.ToString().ToLower() };
                p.IndustryId = industry;
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return _mapper.Map<DataRecipient[]>(allParticipants);
        }

        public async Task<DataRecipientV2[]> GetDataRecipientsAsyncXV2(Industry industry)
        {
            // UNKNOWN industry type is parsed to NOT use industry filtering.
            List<Participation> allParticipants = await ProcessGetDataRecipients(industry);

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipants.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return _mapper.Map<DataRecipientV2[]>(allParticipants);
        }

        public async Task<DataRecipientV3[]> GetDataRecipientsAsyncXV3(Industry industry)
        {
            List<Participation> allParticipants = await ProcessGetDataRecipients(industry);

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipants.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return _mapper.Map<DataRecipientV3[]>(allParticipants);
        }

        /// <remarks>
        /// The industry parameter is passed but currently not used.
        /// </remarks>
        protected async Task<List<Participation>> ProcessGetDataRecipients(Industry industry)
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
                .Where(p => p.ParticipationTypeId == ParticipationTypes.Dr)
                .OrderBy(p => p.LegalEntityId)
                .ToListAsync();
        }

        public async Task<Domain.Entities.SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId)
        {
            var softwareProduct = await _registerDatabaseContext.SoftwareProducts.AsNoTracking()
                .Include(softwareProduct => softwareProduct.Status)
                .Include(softwareProduct => softwareProduct.Brand.BrandStatus)
                .Include(softwareProduct => softwareProduct.Brand.Participation.Status)
                .Where(softwareProduct =>
                    softwareProduct.SoftwareProductId == softwareProductId
                    && softwareProduct.Brand.Participation.ParticipationTypeId == ParticipationTypes.Dr
                    && softwareProduct.StatusId == SoftwareProductStatusType.Active
                    && softwareProduct.Brand.BrandStatusId == BrandStatusType.Active
                    && softwareProduct.Brand.Participation.StatusId == ParticipationStatusType.Active)
                .FirstOrDefaultAsync();

            return _mapper.Map<Domain.Entities.SoftwareProduct>(softwareProduct);
        }
    }
}