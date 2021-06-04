using System;
using System.Linq;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CDR.Register.Repository
{
    public class SoftwareStatementAssertionRepository : ISoftwareStatementAssertionRepository
    {
        private readonly RegisterDatabaseContext _registerDatabaseContext;
        private readonly IRepositoryMapper _mapper;

        public SoftwareStatementAssertionRepository(RegisterDatabaseContext registerDatabaseContext, IRepositoryMapper mapper)
        {
            _registerDatabaseContext = registerDatabaseContext;
            _mapper = mapper;
        }

        public async Task<SoftwareStatementAssertion> GetSoftwareStatementAssertionAsync(Guid dataRecipientBrandId, Guid softwareProductId)
        {
            return await _registerDatabaseContext.SoftwareProducts
                                    .Include(x => x.Certificates)
                                    .Include(x => x.Brand).ThenInclude(x => x.Participation).ThenInclude(x => x.LegalEntity)
                                    .Where(x => x.Brand.BrandId == dataRecipientBrandId && x.SoftwareProductId == softwareProductId)
                                    .Select(x => new SoftwareStatementAssertion
                                    {
                                        DataRecipientBrand = _mapper.Map(x.Brand),
                                        SoftwareProduct = _mapper.MapSoftwareProduct(x),
                                        LegalEntity = _mapper.Map(x.Brand.Participation.LegalEntity)
                                    })
                                    .FirstOrDefaultAsync();

        }
    }
}
