using AutoMapper;
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

        public async Task<Page<DataHolderBrandV1[]>> GetDataHolderBrandsAsyncV1(IndustryEnum industry, DateTime? updatedSince, int page, int pageSize)
        {
            (List<Entities.Brand> allBrands, int totalRecords) = await ProcessGetDataHolderBrands(industry, updatedSince, page, pageSize);

            var result = new Page<DataHolderBrandV1[]>()
            {
                Data = _mapper.Map<DataHolderBrandV1[]>(allBrands),
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
            return result;
        }

        public async Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsync(IndustryEnum industry, DateTime? updatedSince, int page, int pageSize)
        {
            (List<Entities.Brand> allBrands, int totalRecords) = await ProcessGetDataHolderBrands(industry, updatedSince, page, pageSize);
            DataHolderBrand[] rtnData = _mapper.Map<DataHolderBrand[]>(allBrands);

            // NB: Below is more applicable to the future schema definition of including the IndustryId at the Brand level
            //     Currently it will only ever have 1 Brand record per DataHolder Record
            //     Must return a string array of Industries, ie each Participant has a unique Industry
            foreach (var dh in rtnData)
            {
                if (dh.DataHolder.Brands.Count == 1)
                {
                    dh.DataHolder.Industries = new List<string> { dh.DataHolder.Industry };
                }
                else
                {
                    var industries = new List<string>();
                    if (!string.IsNullOrEmpty(dh.DataHolder.Industry))
                        industries.Add(dh.DataHolder.Industry);

                    dh.DataHolder.Industries = industries;
                }
            }

            var result = new Page<DataHolderBrand[]>()
            {
                Data = rtnData,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
            return result;
        }

        protected async Task<(List<Entities.Brand>, int)> ProcessGetDataHolderBrands(IndustryEnum industry, DateTime? updatedSince, int page, int pageSize)
        {
            var allBrandsQuery = this._registerDatabaseContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Where(brand => brand.Participation.ParticipationTypeId == ParticipationTypeEnum.Dh);

            // Add the optional Industry
            if (industry != IndustryEnum.UNKNOWN)
            {
                allBrandsQuery = allBrandsQuery.Where(brand => brand.Participation.IndustryId == industry);
            }

            // Add the updated since filter
            if (updatedSince.HasValue)
            {
                allBrandsQuery = allBrandsQuery.Where(brand => brand.LastUpdated > updatedSince.Value);
            }

            var totalRecords = await allBrandsQuery.CountAsync();

            // Apply ordering and pagination
            allBrandsQuery = allBrandsQuery
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var allBrands = await allBrandsQuery.ToListAsync();

            return (allBrands, totalRecords);
        }

        public async Task<DataRecipientV1[]> GetDataRecipientsAsyncV1(IndustryEnum industry)
        {
            // UNKNOWN industry type is parsed to NOT use industry filtering.
            List<Participation> allParticipents = await ProcessGetDataRecipients(industry);

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipents.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return _mapper.Map<DataRecipientV1[]>(allParticipents);
        }

        public async Task<DataRecipient[]> GetDataRecipientsAsync()
        {
            // UNKNOWN industry type is parsed to NOT use industry filtering.
            List<Participation> allParticipents = await ProcessGetDataRecipients(IndustryEnum.UNKNOWN);

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipents.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return _mapper.Map<DataRecipient[]>(allParticipents);
        }

        protected async Task<List<Participation>> ProcessGetDataRecipients(IndustryEnum industry)
        {
            var allParticipentsQuery = this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Industry)
                .Include(p => p.LegalEntity)
                .Include(p => p.Brands)
                .ThenInclude(brand => brand.BrandStatus)
                .Include(p => p.Brands)
                .ThenInclude(brand => brand.SoftwareProducts)
                .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr);

            // Add the optional Industry
            if (industry != IndustryEnum.UNKNOWN)
            {
                allParticipentsQuery = allParticipentsQuery.Where(brand => brand.IndustryId == industry);
            }

            var totalRecords = await allParticipentsQuery.CountAsync();

            // Apply ordering
            allParticipentsQuery = allParticipentsQuery.OrderBy(p => p.LegalEntityId);

            var allParticipents = await allParticipentsQuery.ToListAsync();
            return allParticipents;
        }

        public async Task<Domain.Entities.SoftwareProduct> GetSoftwareProductIdByIndustryAsync(IndustryEnum industry, Guid softwareProductId)
        {
            var softwareProduct = await _registerDatabaseContext.SoftwareProducts.AsNoTracking()
                .Include(softwareProduct => softwareProduct.Status)
                .Include(softwareProduct => softwareProduct.Brand.BrandStatus)
                .Include(softwareProduct => softwareProduct.Brand.Participation.Status)
                .Where(softwareProduct =>
                    softwareProduct.SoftwareProductId == softwareProductId
                    && softwareProduct.Brand.Participation.ParticipationTypeId == ParticipationTypeEnum.Dr
                    && softwareProduct.StatusId == SoftwareProductStatusEnum.Active
                    && softwareProduct.Brand.BrandStatusId == BrandStatusEnum.Active
                    && softwareProduct.Brand.Participation.StatusId == ParticipationStatusEnum.Active
                    && softwareProduct.Brand.Participation.IndustryId == industry)
                .FirstOrDefaultAsync();

            return _mapper.Map<Domain.Entities.SoftwareProduct>(softwareProduct);
        }

        public async Task<Domain.Entities.SoftwareProduct> GetSoftwareProductIdAsync(Guid softwareProductId)
        {
            var softwareProduct = await _registerDatabaseContext.SoftwareProducts.AsNoTracking()
                .Include(softwareProduct => softwareProduct.Status)
                .Include(softwareProduct => softwareProduct.Brand.BrandStatus)
                .Include(softwareProduct => softwareProduct.Brand.Participation.Status)
                .Where(softwareProduct =>
                    softwareProduct.SoftwareProductId == softwareProductId
                    && softwareProduct.Brand.Participation.ParticipationTypeId == ParticipationTypeEnum.Dr
                    && softwareProduct.StatusId == SoftwareProductStatusEnum.Active
                    && softwareProduct.Brand.BrandStatusId == BrandStatusEnum.Active
                    && softwareProduct.Brand.Participation.StatusId == ParticipationStatusEnum.Active)
                .FirstOrDefaultAsync();

            return _mapper.Map<Domain.Entities.SoftwareProduct>(softwareProduct);
        }
    }
}