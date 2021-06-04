using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;

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

        public async Task<DataRecipientStatus[]> GetDataRecipientStatusesAsync(Industry industry)
        {
            var allParticipents = await this._registerDatabaseContext.Participations.AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.ParticipationTypeId == ParticipationTypeEnum.Dr)
                .OrderBy(p => p.ParticipationId)
                .ToListAsync();

            var res = new List<DataRecipientStatus>();
            foreach (var p in allParticipents)
            {
                res.Add(new DataRecipientStatus
                {
                    DataRecipientId = p.LegalEntityId,
                    Status = p.Status.ParticipationStatusCode
                });
            }
            return res.ToArray();
        }

        public async Task<Domain.Entities.SoftwareProductStatus[]> GetSoftwareProductStatusesAsync(Industry industry)
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
