using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Status.API.Business.Models;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.Status.API.Business
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DataRecipientStatus
            CreateMap<DataRecipientStatus, RegisterDataRecipientStatusModel>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.DataRecipientId))
                .ForMember(dest => dest.DataRecipientStatus, source => source.MapFrom(source => source.Status));

            CreateMap<DataRecipientStatus[], ResponseRegisterDataRecipientStatusList>()
                .ForMember(dest => dest.DataRecipients, source => source.MapFrom(source => source));

            CreateMap<DataRecipientStatus, RegisterDataRecipientStatusModelV2>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.DataRecipientId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<DataRecipientStatus[], ResponseRegisterDataRecipientStatusListV2>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // SoftwareProductStatus
            CreateMap<SoftwareProductStatus, RegisterSoftwareProductStatusModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.SoftwareProductStatus, source => source.MapFrom(source => source.Status));

            CreateMap<SoftwareProductStatus[], ResponseRegisterSoftwareProductStatusList>()
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source));

            CreateMap<SoftwareProductStatus, RegisterSoftwareProductStatusModelV2>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<SoftwareProductStatus[], ResponseRegisterSoftwareProductStatusListV2>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // DataHolderStatus
            CreateMap<DataHolderStatus, RegisterDataHolderStatusModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntityId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<DataHolderStatus[], ResponseRegisterDataHolderStatusList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));
        }
    }
}
