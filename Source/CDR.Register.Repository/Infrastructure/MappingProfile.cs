using AutoMapper;
using CDR.Register.Repository.Entities;
using DomainEntities = CDR.Register.Domain.Entities;

namespace CDR.Register.Repository.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Repo Entity -> Common, DataHolderAuthentications
            CreateMap<AuthDetail, DomainEntities.DataHolderAuthentication>()
                .ForMember(dest => dest.RegisterUType, source => source.MapFrom(source => source.RegisterUType.RegisterUTypeCode))
                .ReverseMap();

            // Repo Entity -> Common, DataHolderBrandServiceEndpoint
            CreateMap<Endpoint, DomainEntities.DataHolderBrandServiceEndpoint>()
                .ReverseMap();


            // Repo Entity -> Multi Industry, DataHolderBrand (Parent)
            CreateMap<Brand, DomainEntities.DataHolderBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusEnum.Active))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => source.AuthDetails))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.Endpoint))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source.Participation))
                .ReverseMap();

            // Repo Entity -> Multi Industry, DataHolderBrand -> DataHolder
            // NB: This is required for V1 backward compatibility to bridge between (Industry) and future versions using (Industries)
            CreateMap<Participation, DomainEntities.DataHolderV1Tmp>()
                .ForMember(dest => dest.DataHolderId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusEnum.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Brands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            // Repo Entity -> Multi Industry, DataHolderBrand -> DataHolder -> LegalEntity
            CreateMap<LegalEntity, DomainEntities.LegalEntityDataHolder>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntityStatusId))
                .ReverseMap();

            // Repo Entity -> Multi Industry, DataHolderBrand -> DataHolder -> Brands
            CreateMap<Brand, DomainEntities.DataHolderBrandTempV1>();


            // Repo Entity -> Legacy, DataHolderBrandV1 (Parent)
            CreateMap<Brand, DomainEntities.DataHolderBrandV1>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusEnum.Active))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => source.AuthDetails))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.Endpoint))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source.Participation))
                .ReverseMap();

            // Repo Entity -> Legacy, DataHolderBrand -> DataHolderV1
            CreateMap<Participation, DomainEntities.DataHolderV1>()
                .ForMember(dest => dest.DataHolderId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusEnum.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.DataHolderBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();


            // Repo Entity -> Multi Industry, DataRecipient (Parent)
            CreateMap<Participation, DomainEntities.DataRecipient>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusEnum.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            // Repo Entity -> Multi Industry, DataRecipient -> LegalEntity
            CreateMap<LegalEntity, DomainEntities.LegalEntityDataRecipient>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntityStatusId))
                .ReverseMap();

            // Repo Entity -> Multi Industry, DataRecipient -> DataRecipientBrands
            // Repo Entity -> Legacy, DataRecipientV1 -> DataRecipientBrands
            CreateMap<Brand, DomainEntities.DataRecipientBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusEnum.Active))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts))
                .ForMember(dest => dest.DataRecipient, source => source.MapFrom(source => source.Participation))
                .ReverseMap();


            // Repo Entity -> Legacy, DataRecipientV1 (Parent)
            CreateMap<Participation, DomainEntities.DataRecipientV1>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusEnum.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            // Repo Entity -> Legacy, DataRecipientV1 -> LegalEntityV1
            CreateMap<LegalEntity, DomainEntities.LegalEntityV1>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.IndustryCode, source => source.MapFrom(source => source.AnzsicDivision))
                .ReverseMap();


            // Repo Entity
            CreateMap<SoftwareProduct, DomainEntities.SoftwareProduct>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.SoftwareProductStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.SoftwareProductStatusId == SoftwareProductStatusEnum.Active))
                .ForMember(dest => dest.RedirectUri, source => source.MapFrom(s => s.RedirectUris))
                .ForMember(dest => dest.DataRecipientBrand, source => source.MapFrom(s => s.Brand))
                .ReverseMap();

            // Repo Entity
            CreateMap<SoftwareProduct, DomainEntities.SoftwareProductIdSvr>()
                .ForMember(dest => dest.Id, source => source.MapFrom(s => s.SoftwareProductId))
                .ForMember(dest => dest.Name, source => source.MapFrom(s => s.SoftwareProductName))
                .ForMember(dest => dest.JwksUri, source => source.MapFrom(s => s.JwksUri))
                .ForMember(dest => dest.X509Certificates, source => source.MapFrom(s => s.Certificates));

            // Repo Entity
            CreateMap<SoftwareProductCertificate, DomainEntities.SoftwareProductCertificateIdSvr>();
        }
    }
}