using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository
{
    public interface IRepositoryMapper
    {
        LegalEntityV1 Map(Entities.LegalEntity legalEntity);
        DataRecipientBrand Map(Entities.Brand brand);
        SoftwareProductIdSvr Map(Entities.SoftwareProduct softwareProduct);
        SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct);
    }
}
