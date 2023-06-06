using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository
{
    public interface IRepositoryMapper
    {
        DataRecipientLegalEntity Map(Entities.LegalEntity legalEntity);
        Entities.LegalEntity Map(DataRecipientLegalEntity legalEntity);

        DataRecipientBrand Map(Entities.Brand brand);
        Entities.Brand Map(DataRecipientBrand brand);

        SoftwareProductInfosec Map(Entities.SoftwareProduct softwareProduct);
        Entities.SoftwareProduct Map(SoftwareProduct softwareProduct);

        SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct);

        Entities.SoftwareProductCertificate Map(SoftwareProductCertificateInfosec softwareProductCertificate);
    }
}
