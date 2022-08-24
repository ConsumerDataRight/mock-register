using AutoMapper;
using CDR.Register.Repository.Entities;
using DomainEntities = CDR.Register.Domain.Entities;

namespace CDR.Register.Repository.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LegalEntity, DomainEntities.DataHolderLegalEntity>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.IndustryCode, source => source.MapFrom(source => source.AnzsicDivision))
                .ReverseMap();

            CreateMap<LegalEntity, DomainEntities.DataHolderLegalEntityV2>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntityStatusId.ToString().ToUpper()))
                .ReverseMap();

            CreateMap<LegalEntity, DomainEntities.DataRecipientLegalEntity>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntityStatusId))
                .ForMember(dest => dest.IndustryCode, source => source.MapFrom(source => source.AnzsicDivision))
                .ReverseMap();

            CreateMap<LegalEntity, DomainEntities.DataRecipientLegalEntityV2>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntityStatus.LegalEntityStatusId.ToString().ToUpper()))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataHolder>()
                .ForMember(dest => dest.DataHolderId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.DataHolderBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataHolderV2>()
                .ForMember(dest => dest.DataHolderId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Brands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataRecipient>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataRecipientV2>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataRecipientV3>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataHolderLegalEntityV2>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.StatusId.ToString().ToUpper()))
                .ReverseMap();

            CreateMap<Brand, DomainEntities.DataHolderBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusType.Active))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => source.AuthDetails))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.Endpoint))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source.Participation))
                .ReverseMap();

            CreateMap<Brand, DomainEntities.DataHolderBrandV2>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusType.Active))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => source.AuthDetails))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.Endpoint))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source.Participation))
                .ReverseMap();

            CreateMap<Brand, DomainEntities.DataRecipientBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusType.Active))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts))
                .ForMember(dest => dest.DataRecipient, source => source.MapFrom(source => source.Participation))
                .ReverseMap();

            CreateMap<SoftwareProduct, DomainEntities.SoftwareProduct>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.SoftwareProductStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.SoftwareProductStatusId == SoftwareProductStatusType.Active))
                .ForMember(dest => dest.RedirectUri, source => source.MapFrom(s => s.RedirectUris))
                .ForMember(dest => dest.DataRecipientBrand, source => source.MapFrom(s => s.Brand))
                .ReverseMap();

            CreateMap<SoftwareProduct, DomainEntities.SoftwareProductInfosec>()
                .ForMember(dest => dest.Id, source => source.MapFrom(s => s.SoftwareProductId))
                .ForMember(dest => dest.Name, source => source.MapFrom(s => s.SoftwareProductName))
                .ForMember(dest => dest.JwksUri, source => source.MapFrom(s => s.JwksUri))
                .ForMember(dest => dest.X509Certificates, source => source.MapFrom(s => s.Certificates));

            CreateMap<SoftwareProductCertificate, DomainEntities.SoftwareProductCertificateInfosec>();

            CreateMap<AuthDetail, DomainEntities.DataHolderAuthentication>()
                .ForMember(dest => dest.RegisterUType, source => source.MapFrom(source => source.RegisterUType.RegisterUTypeCode))
                .ReverseMap();

            CreateMap<Endpoint, DomainEntities.DataHolderBrandServiceEndpoint>()
                .ReverseMap();
        }
    }
}