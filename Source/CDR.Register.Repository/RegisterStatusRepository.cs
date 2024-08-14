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

        public async Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Infrastructure.Industry industry)
        {
            return await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypes.Dr)
                .Select(p => new DataRecipientStatus() { DataRecipientId = p.LegalEntityId, Status = p.Status.ParticipationStatusCode })
                .OrderBy(p => p.DataRecipientId.ToString())
                .ToArrayAsync();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Infrastructure.Industry industry)
        {
            var allParticipants = await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Industry)
                .Include(p => p.Brands)
                .ThenInclude(brand => brand.SoftwareProducts)
                .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypes.Dr)
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
                .OrderBy(sp => sp.SoftwareProductId)
                .ToArray();
        }

        public async Task<DataHolderStatus[]> GetDataHolderStatusesAsync(Infrastructure.Industry industry)
        {
            return await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Include(p => p.Industry)
                .Include(p => p.LegalEntity)
                .Where(p => p.ParticipationTypeId == ParticipationTypes.Dh)
                .Where(p => industry == Infrastructure.Industry.ALL || p.IndustryId == industry)
                .Where(p => p.StatusId == ParticipationStatusType.Active)
                .Select(p => new DataHolderStatus() { LegalEntityId = p.LegalEntityId, Status = p.Status.ParticipationStatusCode })
                .OrderBy(p => p.LegalEntityId)
                .ToArrayAsync();
        }
    }
}
