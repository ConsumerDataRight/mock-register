using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository
{
    public interface IRepositoryMapper
    {
        DataRecipientLegalEntity Map(Entities.LegalEntity legalEntity);
        DataRecipientBrand Map(Entities.Brand brand);
        SoftwareProductInfosec Map(Entities.SoftwareProduct softwareProduct);
        SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct);
    }
}
