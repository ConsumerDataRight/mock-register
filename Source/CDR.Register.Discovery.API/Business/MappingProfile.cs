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
            CreateMap(typeof(Page<>), typeof(MetaPaginated));

            CreateMap<LegalEntity, LegalEntityModel>()
                .ForMember(dest => dest.RegistrationDate,
                    source => source.MapFrom(source => source.RegistrationDate.HasValue ? source.RegistrationDate.Value.ToString("yyyy-MM-dd") : null));

            CreateMap<DataHolderAuthentication, AuthDetailModel>();

            CreateMap<DataHolderBrandServiceEndpoint, EndpointDetailModel>();

            CreateMap<DataHolderBrand, RegisterDataHolderBrandModel>()
                .ForMember(dest => dest.DataHolderBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.AuthDetails, source => source.MapFrom(source => source.DataHolderAuthentications))
                .ForMember(dest => dest.EndPointDetail, source => source.MapFrom(source => source.DataHolderBrandServiceEndpoint))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.DataHolder.LegalEntity));

            CreateMap<SoftwareProduct, SoftwareProductModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.SoftwareProductName, source => source.MapFrom(source => source.SoftwareProductName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<DataRecipientBrand, DataRecipientBrandModel>()
                .ForMember(dest => dest.DataRecipientBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.BrandName, source => source.MapFrom(source => source.BrandName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts));

            CreateMap<DataRecipient, RegisterDataRecipientModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(dest => dest.Industry, source => source.MapFrom(source => source.Industry))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LegalEntity.LogoUri))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands));

            CreateMap<DataRecipient, RegisterDataRecipientModelV2>()
                .IncludeBase<DataRecipient, RegisterDataRecipientModel>()
                .ForMember(dest => dest.AccreditationNumber, source => source.MapFrom(source => source.LegalEntity.AccreditationNumber));

            CreateMap<DataRecipient[], ResponseRegisterDataRecipientList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            CreateMap<Page<DataHolderBrand[]>, ResponseRegisterDataHolderBrandList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source.Data))
                .ForMember(dest => dest.Meta, source => source.MapFrom(source => source));

            CreateMap<DataRecipient[], ResponseRegisterDataRecipientListV2>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));
        }
    }
}
