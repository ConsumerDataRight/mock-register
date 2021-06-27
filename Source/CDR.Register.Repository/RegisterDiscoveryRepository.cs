using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Domain.ValueObjects;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Page<DataHolderBrand[]>> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var allParticipentsQuery = this._registerDatabaseContext.Brands.AsNoTracking()
                .Include(brand => brand.Endpoint)
                .Include(brand => brand.BrandStatus)
                .Include(brand => brand.AuthDetails)
                .ThenInclude(authDetail => authDetail.RegisterUType)
                .Include(brand => brand.Participation.LegalEntity.OrganisationType)
                .Include(brand => brand.Participation.Industry)
                .Where(brand => brand.Participation.ParticipationTypeId == ParticipationTypeEnum.Dh);

            // Add the updated since filter
            if (updatedSince.HasValue)
            {
                allParticipentsQuery = allParticipentsQuery.Where(brand => brand.LastUpdated > updatedSince.Value);
            }

            var totalRecords = await allParticipentsQuery.CountAsync();

            // Apply ordering and pagination
            allParticipentsQuery = allParticipentsQuery
                .OrderBy(brand => brand.BrandName).ThenBy(brand => brand.BrandId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var allParticipents = await allParticipentsQuery.ToListAsync();

            var result = new Page<DataHolderBrand[]>()
            {
                Data = _mapper.Map<DataHolderBrand[]>(allParticipents),
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
            return result;
        }

        public async Task<DataRecipient[]> GetDataRecipientsAsync(Industry industry)
        {
            var allParticipents = await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(participation => participation.Status)
                .Include(participation => participation.Industry)
                .Include(participation => participation.LegalEntity)
                .Include(participation => participation.Brands)
                .ThenInclude(brand => brand.BrandStatus)
                .Include(participation => participation.Brands)
                .ThenInclude(brand => brand.SoftwareProducts)
                .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .OrderBy(p => p.LegalEntityId)  // Sort by id
                .ToListAsync();

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
