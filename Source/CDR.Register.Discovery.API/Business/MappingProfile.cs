using AutoMapper;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.ValueObjects;

namespace CDR.Register.Discovery.API.Business
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Domain Entity -> Common, Data -> Meta
            CreateMap(typeof(Page<>), typeof(MetaPaginated));

            // Domain Entity -> Common, Data -> AuthDetails
            CreateMap<DataHolderAuthentication, AuthDetailModel>();

            // Domain Entity -> Common, Data -> EndPointDetail
            CreateMap<DataHolderBrandServiceEndpoint, EndpointDetailModel>();

            CreateMap<LegalEntityDataRecipient, LegalEntityDataRecipientModel>();

            // Domain Entity -> Multi Industry, Returned List
            CreateMap<Page<DataHolderBrand[]>, ResponseRegisterDataHolderBrandList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source.Data))
                .ForMember(dest => dest.Meta, source => source.MapFrom(source => source));

            // Domain Entity -> Multi Industry, Data
            CreateMap<DataHolderBrand, RegisterDataHolderBrandModel>()
                .ForMember(dest => dest.DataHolderBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.Industries, source => source.MapFrom(source => source.DataHolder.Industries))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.AuthDetails, source => source.MapFrom(source => source.DataHolderAuthentications))
                .ForMember(dest => dest.EndPointDetail, source => source.MapFrom(source => source.DataHolderBrandServiceEndpoint))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.DataHolder.LegalEntity));

            // Domain Entity -> Multi Industry, Data -> LegalEntity
            CreateMap<LegalEntityDataHolder, LegalEntityDataHolderModel>()
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
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            // Domain Entity -> Legacy, Returned List
            CreateMap<Page<DataHolderBrandV1[]>, ResponseRegisterDataHolderBrandListV1>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source.Data))
                .ForMember(dest => dest.Meta, source => source.MapFrom(source => source));

            // Domain Entity -> Legacy, Data
            CreateMap<DataHolderBrandV1, RegisterDataHolderBrandModelV1>()
                .ForMember(dest => dest.DataHolderBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.DataHolder.Industry))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.AuthDetails, source => source.MapFrom(source => source.DataHolderAuthentications))
                .ForMember(dest => dest.EndPointDetail, source => source.MapFrom(source => source.DataHolderBrandServiceEndpoint))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.DataHolder.LegalEntity));

            // Domain Entity -> Legacy, Data -> LegalEntity
            CreateMap<LegalEntityV1, LegalEntityModelV1>()
                .ForMember(dest => dest.RegistrationDate, source => source.MapFrom(source => source.RegistrationDate.HasValue ? source.RegistrationDate.Value.ToString("yyyy-MM-dd") : null));


            // Domain Entity - Common, DataRecipientBrands
            CreateMap<DataRecipientBrand, DataRecipientBrandModel>()
                .ForMember(dest => dest.DataRecipientBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.BrandName, source => source.MapFrom(source => source.BrandName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts));


            // Domain Entity - Multi Industry, Data Recipient List (Parent)
            CreateMap<DataRecipient[], ResponseRegisterDataRecipientList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // Domain Entity - Multi Industry, Data
            CreateMap<DataRecipient, RegisterDataRecipient>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LegalEntity.LogoUri))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands))
                .ForMember(dest => dest.AccreditationNumber, source => source.MapFrom(source => source.LegalEntity.AccreditationNumber))
                .ForMember(dest => dest.AccreditationLevel, source => source.MapFrom(source => source.LegalEntity.AccreditationLevelId));


            // Domain Entity - Legacy, Data Recipient List V1 (Parent)
            CreateMap<DataRecipientV1[], ResponseRegisterDataRecipientListV1>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // Domain Entity - Legacy, Data V1
            CreateMap<DataRecipientV1, RegisterDataRecipientModelV1>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(dest => dest.AccreditationNumber, source => source.MapFrom(source => source.LegalEntity.AccreditationNumber))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LegalEntity.LogoUri))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status))
                .ForMember(dest => dest.LastUpdated, source => source.MapFrom(source => source.LastUpdated));


            // Domain Entity - Legacy, Data Recipient Returned List V2 (Parent)
            CreateMap<DataRecipient[], ResponseRegisterDataRecipientListV2>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // Domain Entity - Legacy, Data V2
            CreateMap<DataRecipient, RegisterDataRecipientV2>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LegalEntity.LogoUri))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands))
                .ForMember(dest => dest.AccreditationNumber, source => source.MapFrom(source => source.LegalEntity.AccreditationNumber))
                .ForMember(dest => dest.AccreditationLevel, source => source.MapFrom(source => source.LegalEntity.AccreditationLevelId));


            // Domain Entity
            CreateMap<SoftwareProduct, SoftwareProductModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.SoftwareProductName, source => source.MapFrom(source => source.SoftwareProductName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));
        }
    }
}