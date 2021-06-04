using AutoMapper;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository.Infrastructure
{
    public class RepositoryMapper : IRepositoryMapper
    {
        private readonly IMapper _mapper;

        public RepositoryMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();
        }

        public SoftwareProductIdSvr Map(Entities.SoftwareProduct softwareProduct)
        {
            return _mapper.Map<SoftwareProductIdSvr>(softwareProduct);
        }

        public DataRecipientBrand Map(Entities.Brand brand)
        {
            return _mapper.Map<DataRecipientBrand>(brand);
        }

        public Domain.Entities.LegalEntity Map(Entities.LegalEntity legalEntity)
        {
            return _mapper.Map<Domain.Entities.LegalEntity>(legalEntity);
        }

        public Domain.Entities.SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct)
        {
            return _mapper.Map<Domain.Entities.SoftwareProduct>(softwareProduct);
        }
    }
}
