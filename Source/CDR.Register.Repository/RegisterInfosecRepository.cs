using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CDR.Register.Repository
{
    public class RegisterInfosecRepository : IRegisterInfosecRepository
    {
        private readonly RegisterDatabaseContext _registerDatabaseContext;
        private readonly IRepositoryMapper _mapper;

        public RegisterInfosecRepository(
            RegisterDatabaseContext registerDatabaseContext,
            IRepositoryMapper mapper)
        {
            this._registerDatabaseContext = registerDatabaseContext;
            this._mapper = mapper;
        }

        public async Task<SoftwareProductInfosec> GetSoftwareProductAsync(Guid id)
        {
            var softwareProduct = await this._registerDatabaseContext.SoftwareProducts
                                                     .Include(x => x.Certificates)
                                                     .FirstOrDefaultAsync(x => x.SoftwareProductId == id);

            return _mapper.Map(softwareProduct);
        }
    }
}
