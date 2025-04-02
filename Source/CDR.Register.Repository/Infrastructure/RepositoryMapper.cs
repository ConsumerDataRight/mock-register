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

        public SoftwareProductInfosec Map(Entities.SoftwareProduct softwareProduct)
        {
            return _mapper.Map<SoftwareProductInfosec>(softwareProduct);
        }

        public DataRecipientBrand Map(Entities.Brand brand)
        {
            return _mapper.Map<DataRecipientBrand>(brand);
        }

        public DataRecipientLegalEntity Map(Entities.LegalEntity legalEntity)
        {
            return _mapper.Map<Domain.Entities.DataRecipientLegalEntity>(legalEntity);
        }

        public Entities.LegalEntity Map(Domain.Entities.DataRecipientLegalEntity legalEntity)
        {
            return _mapper.Map<Entities.LegalEntity>(legalEntity);
        }

        public Entities.Brand Map(DataRecipientBrand brand)
        {
            return _mapper.Map<Entities.Brand>(brand);
        }

        public Entities.SoftwareProduct Map(Domain.Entities.SoftwareProduct softwareProduct)
        {
            return _mapper.Map<Entities.SoftwareProduct>(softwareProduct);
        }

        public Entities.SoftwareProductCertificate Map(SoftwareProductCertificateInfosec softwareProductCertificate)
        {
            return _mapper.Map<Entities.SoftwareProductCertificate>(softwareProductCertificate);
        }

        public Domain.Entities.SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct)
        {
            return _mapper.Map<Domain.Entities.SoftwareProduct>(softwareProduct);
        }
    }
}
