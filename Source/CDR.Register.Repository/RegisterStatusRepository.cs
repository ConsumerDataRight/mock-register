using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CDR.Register.Repository
{
    public class RegisterStatusRepository : IRegisterStatusRepository
    {
        private readonly RegisterDatabaseContext _registerDatabaseContext;

        public RegisterStatusRepository(
            RegisterDatabaseContext registerDatabaseContext)
        {
            this._registerDatabaseContext = registerDatabaseContext;
        }

        public async Task<DataRecipientStatusV1[]> GetDataRecipientStatusesAsyncV1()
        {
            return await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .OrderBy(p => p.ParticipationId)
                .Select(p => new DataRecipientStatusV1() { DataRecipientId = p.LegalEntityId, DataRecipientStatus = p.Status.ParticipationStatusCode })
                .ToArrayAsync();
        }

        /// <remarks>
        /// industry parameter is passed but is not currently used.
        /// </remarks>
        public async Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Industry industry)
        {
            return await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .OrderBy(p => p.ParticipationId)
                .Select(p => new DataRecipientStatus() { LegalEntityId = p.LegalEntityId, Status = p.Status.ParticipationStatusCode })
                .ToArrayAsync();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsyncV1()
        {            
            return await GetSoftwareProductStatusesAsync();
        }

        /// <remarks>
        /// industry parameter is passed but is not currently used.
        /// </remarks>
        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Industry industry)
        {
            var allParticipants = await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Industry)
                .Include(p => p.Brands)
                .ThenInclude(brand => brand.SoftwareProducts)
                .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .ToListAsync();

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipants.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            return allParticipants
                .SelectMany(p => p.Brands)
                .SelectMany(b => b.SoftwareProducts)
                .Select(sp => 
                    new Domain.Entities.SoftwareProductStatus
                    {
                        SoftwareProductId = sp.SoftwareProductId,
                        Status = sp.Status.SoftwareProductStatusCode
                    }
                )
                .ToArray();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync()
        {
            return await this._registerDatabaseContext.SoftwareProducts.AsNoTracking()
                .Include(p => p.Status)
                .OrderBy(p => p.SoftwareProductId)
                .Select(p => new Domain.Entities.SoftwareProductStatus() { SoftwareProductId = p.SoftwareProductId, Status = p.Status.SoftwareProductStatusCode })
                .ToArrayAsync();
        }
    }
}
