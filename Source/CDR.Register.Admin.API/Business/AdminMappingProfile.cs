using System.Collections.Generic;
using AutoMapper;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using CDR.Register.Repository.Enums;
using Microsoft.Extensions.Configuration;
using DomainEntities = CDR.Register.Domain.Entities;

namespace CDR.Register.Admin.API.Business
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            this.CreateMap<Business.Model.LegalEntity, DomainEntities.DataRecipient>()
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands));

            this.CreateMap<Business.Model.LegalEntity, DomainEntities.DataRecipientLegalEntity>()
                .ForMember(dest => dest.AccreditationLevelId, source => source.MapFrom(source => source.AccreditationLevel));

            this.CreateMap<Business.Model.Brand, DomainEntities.DataRecipientBrand>()
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.Status))
                .ForMember(dest => dest.BrandId, source => source.MapFrom(source => source.DataRecipientBrandId));

            this.CreateMap<Business.Model.SoftwareProduct, DomainEntities.SoftwareProduct>()
                .ForMember(dest => dest.RedirectUri, source => source.MapFrom(src => src.RedirectUris != null ? string.Join(" ", src.RedirectUris) : string.Empty))
                .ForMember(dest => dest.RedirectUris, opts => opts.Ignore()) // Ignore this as it is a computed property with no setter
                .ForMember(dest => dest.Scope, opt => opt.MapFrom<SoftwareScopeResolver, string>(src => src.Scope ?? string.Empty));

            this.CreateMap<Business.Model.SoftwareProductCertificate, DomainEntities.SoftwareProductCertificateInfosec>();

            // DH Brand Mappings
            this.CreateMap<DataHolderAuthenticationModel, DataHolderAuthentication>();
            this.CreateMap<DataHolderEndpointModel, DataHolderBrandServiceEndpoint>();
            this.CreateMap<DataHolderEndpointModel, DataHolderBrandServiceEndpoint>();

            this.CreateMap<DataHolderLegalEntityModel, DataHolderLegalEntity>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntityName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.RegistrationNumber, source => source.MapFrom(source => source.RegistrationNumber))
                .ForMember(dest => dest.RegistrationDate, source => source.MapFrom(source => source.RegistrationDate.HasValue ? source.RegistrationDate.Value.ToString("yyyy-MM-dd") : null))
                .ForMember(dest => dest.RegisteredCountry, source => source.MapFrom(source => source.RegisteredCountry))
                .ForMember(dest => dest.Abn, source => source.MapFrom(source => source.Abn))
                .ForMember(dest => dest.Acn, source => source.MapFrom(source => source.Acn))
                .ForMember(dest => dest.Arbn, source => source.MapFrom(source => source.Arbn))
                .ForMember(dest => dest.AnzsicDivision, source => source.MapFrom(source => source.AnzsicDivision))
                .ForMember(dest => dest.OrganisationType, source => source.MapFrom(source => source.OrganisationType))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status.ToUpper()));

            this.CreateMap<DataHolderBrandModel, DataHolder>()
                .ForMember(dest => dest.Industries, (IMemberConfigurationExpression<DataHolderBrandModel, DataHolder, List<string>> source) => source.MapFrom(source => source.Industries))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industries.Length > 0 ? source.Industries[0] : string.Empty))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source == null ? null : source.LegalEntity))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.LegalEntity == null ? string.Empty : source.LegalEntity.Status.ToUpper()));

            this.CreateMap<DataHolderBrandModel, DataHolderBrand>()
                .ForMember(dest => dest.BrandId, source => source.MapFrom(source => source.DataHolderBrandId))
                .ForMember(dest => dest.BrandName, source => source.MapFrom(source => source.BrandName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.BrandStatus, source => source.MapFrom(source => source.Status.ToUpper()))
                .ForMember(dest => dest.IsActive, source => source.MapFrom(source => string.Compare(source.Status, BrandStatusType.Active.ToString(), true)))
                .ForMember(dest => dest.DataHolderAuthentications, source => source.MapFrom(source => new[] { source.AuthDetails }))
                .ForMember(dest => dest.DataHolderBrandServiceEndpoint, source => source.MapFrom(source => source.EndpointDetail))
                .ForMember(dest => dest.DataHolder, source => source.MapFrom(source => source));
        }
    }
}
