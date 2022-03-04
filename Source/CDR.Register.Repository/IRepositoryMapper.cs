﻿using CDR.Register.Domain.Entities;

namespace CDR.Register.Repository
{
    public interface IRepositoryMapper
    {
        SoftwareProductIdSvr Map(Entities.SoftwareProduct softwareProduct);
        DataRecipientBrand Map(Entities.Brand brand);
        SoftwareProduct MapSoftwareProduct(Entities.SoftwareProduct softwareProduct);
        LegalEntityV1 Map(Entities.LegalEntity legalEntity);
    }
}
