using AutoMapper;
using CDR.Register.Repository.Entities;
using System;
using System.Linq;
using DomainEntities = CDR.Register.Domain.Entities;

namespace CDR.Register.Repository.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LegalEntity, DomainEntities.DataHolderLegalEntity>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Participations.FirstOrDefault().Status.ParticipationStatusCode.ToUpper()));
            CreateMap<DomainEntities.DataHolderLegalEntity, LegalEntity>()
                .ForMember(dest => dest.OrganisationTypeId, source => source.MapFrom(source =>
                    source.OrganisationType == null ? null : Enum.Parse(typeof(OrganisationTypes), source.OrganisationType.Replace("_", string.Empty), true)))
                .ForMember(dest => dest.OrganisationType, opt => opt.Ignore());

            CreateMap<LegalEntity, DomainEntities.DataRecipientLegalEntity>()
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType.OrganisationTypeCode))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Participations.FirstOrDefault().Status.ParticipationStatusCode.ToUpper()));

            CreateMap<DomainEntities.DataRecipientLegalEntity, LegalEntity>()
                .ForMember(dest => dest.OrganisationTypeId, source => source.MapFrom(source => source.OrganisationType == null ? null : Enum.Parse(typeof(Entities.OrganisationTypes), source.OrganisationType.Replace("_", ""), true)))
                .ForMember(dest => dest.OrganisationType, opt => opt.Ignore());


            CreateMap<Participation, DomainEntities.DataHolder>()
                .ForMember(dest => dest.DataHolderId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry.IndustryTypeCode))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Brands, source => source.MapFrom(source => source.Brands));
            CreateMap<DomainEntities.DataHolder, Participation>()
                .ForMember(dest => dest.StatusId, source => source.MapFrom(source => Enum.Parse(typeof(ParticipationStatusType), source.Status, true)))
                .ForMember(dest => dest.IndustryId, source => source.MapFrom(source => Enum.Parse(typeof(Industry), source.Industry, true)))
                .ForMember(dest => dest.ParticipationTypeId, source => source.MapFrom(source => ParticipationTypes.Dh)) // This is a Dh Participation
                .ForMember(dest => dest.Industry, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ParticipationType, opt => opt.Ignore());

            CreateMap<Participation, DomainEntities.DataRecipient>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.ParticipationId))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.ParticipationStatusId == ParticipationStatusType.Active))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.LegalEntity))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ParticipationStatusCode))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.Brands))
                .ReverseMap();

            CreateMap<Participation, DomainEntities.DataHolderLegalEntity>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.StatusId.ToString().ToUpper()))
                .ReverseMap();

            CreateMap<Brand, DomainEntities.DataHolderBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusType.Active))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => source.AuthDetails))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.Endpoint))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source.Participation));
            CreateMap<DomainEntities.DataHolderBrand, Brand>()
                .ForMember(dest => dest.BrandName, source => source.MapFrom(source => source.BrandName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.BrandStatusId, source => source.MapFrom(source => Enum.Parse(typeof(BrandStatusType), source.BrandStatus, true)))
                .ForMember(dest => dest.BrandStatus, opt => opt.Ignore());

            CreateMap<Brand, DomainEntities.DataRecipientBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.BrandStatus.BrandStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.BrandStatus.BrandStatusId == BrandStatusType.Active))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts))
                .ForMember(dest => dest.DataRecipient, source => source.MapFrom(source => source.Participation));

            CreateMap<DomainEntities.DataRecipientBrand, Brand>()
                .ForMember(dest => dest.BrandStatusId, source => source.MapFrom(source => Enum.Parse(typeof(BrandStatusType), source.BrandStatus, true)))
                .ForMember(dest => dest.BrandStatus, opts => opts.Ignore())
                .ForMember(dest => dest.SoftwareProducts, opts => opts.Ignore());


            CreateMap<SoftwareProduct, DomainEntities.SoftwareProduct>()
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.SoftwareProductStatusCode))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => source.Status.SoftwareProductStatusId == SoftwareProductStatusType.Active))
                .ForMember(dest => dest.RedirectUri, source => source.MapFrom(src => src.RedirectUris))
                .ForMember(dest => dest.RedirectUris, opts => opts.Ignore()) //Ignore this as it is a computed property with no setter
                .ForMember(dest => dest.DataRecipientBrand, source => source.MapFrom(s => s.Brand));

            CreateMap<DomainEntities.SoftwareProduct, SoftwareProduct>()
                .ForMember(dest => dest.StatusId, source => source.MapFrom(source => Enum.Parse(typeof(Entities.SoftwareProductStatusType), source.Status, true)))
                .ForMember(dest => dest.RedirectUris, source => source.MapFrom(src => src.RedirectUris != null ? string.Join(" ", src.RedirectUris) : string.Empty))
                .ForMember(dest => dest.Status, opts => opts.Ignore())
                .ForMember(dest => dest.Certificates, opts => opts.Ignore());

            CreateMap<SoftwareProduct, DomainEntities.SoftwareProductInfosec>()
                .ForMember(dest => dest.Id, source => source.MapFrom(s => s.SoftwareProductId))
                .ForMember(dest => dest.Name, source => source.MapFrom(s => s.SoftwareProductName))
                .ForMember(dest => dest.JwksUri, source => source.MapFrom(s => s.JwksUri))
                .ForMember(dest => dest.X509Certificates, source => source.MapFrom(s => s.Certificates));

            CreateMap<SoftwareProductCertificate, DomainEntities.SoftwareProductCertificateInfosec>().ReverseMap();

            CreateMap<AuthDetail, DomainEntities.DataHolderAuthentication>()
                .ForMember(dest => dest.RegisterUType, source => source.MapFrom(source => source.RegisterUType.RegisterUTypeCode));
            CreateMap<DomainEntities.DataHolderAuthentication, AuthDetail>()
                .ForMember(dest => dest.RegisterUTypeId, source => source.MapFrom(source => 
                    Enum.Parse(typeof(RegisterUTypes), source.RegisterUType.Replace("-", string.Empty), true)))
                .ForMember(dest => dest.JwksEndpoint, source => source.MapFrom(source => source.JwksEndpoint))
                .ForMember(dest => dest.RegisterUType, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.BrandId, opt => opt.Ignore());

            CreateMap<Endpoint, DomainEntities.DataHolderBrandServiceEndpoint>()
                .ReverseMap();
        }
    }
}