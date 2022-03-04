using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            var allParticipents = await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .OrderBy(p => p.ParticipationId)
                .ToListAsync();

            var res = new List<DataRecipientStatusV1>();
            foreach (var p in allParticipents)
            {
                res.Add(new DataRecipientStatusV1
                {
                    DataRecipientId = p.LegalEntityId,
                    DataRecipientStatus = p.Status.ParticipationStatusCode
                });
            }
            return res.ToArray();
        }

        public async Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(IndustryEnum industry)
        {
            var allParticipentsQuery = this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr);

            // Add the optional Industry
            if (industry != IndustryEnum.UNKNOWN)
            {
                allParticipentsQuery = allParticipentsQuery.Where(p => p.IndustryId == industry);
            }

            // Apply ordering
            allParticipentsQuery = allParticipentsQuery.OrderBy(p => p.ParticipationId);

            // Return Data
            var allParticipents = await allParticipentsQuery.ToListAsync();

            var res = new List<DataRecipientStatus>();
            foreach (var p in allParticipents)
            {
                res.Add(new DataRecipientStatus
                {
                    LegalEntityId = p.LegalEntityId,
                    Status = p.Status.ParticipationStatusCode
                });
            }
            return res.ToArray();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsyncV1()
        {            
            return await GetSoftwareProductStatusesAsync();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(IndustryEnum industry)
        {
            var allParticipentsQuery = this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Industry)
                .Include(p => p.Brands)
                .ThenInclude(brand => brand.SoftwareProducts)
                .ThenInclude(softwareProduct => softwareProduct.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr);

            // Add the optional Industry
            if (industry != IndustryEnum.UNKNOWN)
            {
                allParticipentsQuery = allParticipentsQuery.Where(brand => brand.IndustryId == industry);
            }

            var allParticipents = await allParticipentsQuery.ToListAsync();

            // Additionally sort participants.brands, participants.brands.softwareproducts by id
            allParticipents.ForEach(p =>
            {
                p.Brands = p.Brands.OrderBy(b => b.BrandId).ToList();
                p.Brands.ToList().ForEach(b =>
                {
                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                });
            });

            var allSoftWareProducts = new List<Domain.Entities.SoftwareProductStatus>();
            foreach (var p in allParticipents)
            {
                foreach (var b in p.Brands)
                {
                    foreach (var s in b.SoftwareProducts)
                    {
                        allSoftWareProducts.Add(new Domain.Entities.SoftwareProductStatus
                        {
                            SoftwareProductId = s.SoftwareProductId,
                            Status = s.Status.SoftwareProductStatusCode
                        });
                    }
                }
            }
            return allSoftWareProducts.ToArray();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync()
        {
            var allSoftwareProducts = await this._registerDatabaseContext.SoftwareProducts.AsNoTracking()
                .Include(p => p.Status)
                .OrderBy(p => p.SoftwareProductId)
                .ToListAsync();

            var res = new List<Domain.Entities.SoftwareProductStatus>();
            foreach (var p in allSoftwareProducts)
            {
                res.Add(new Domain.Entities.SoftwareProductStatus
                {
                    SoftwareProductId = p.SoftwareProductId,
                    Status = p.Status.SoftwareProductStatusCode
                });
            }
            return res.ToArray();
        }
    }
}
