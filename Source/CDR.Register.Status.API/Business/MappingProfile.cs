﻿using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.Status.API.Business.Models;
using CDR.Register.Status.API.Business.Responses;

namespace CDR.Register.SSA.API.Business
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DataRecipientStatus
            CreateMap<DataRecipientStatusV1, RegisterDataRecipientStatusModelV1>()
                .ForMember(dest => dest.DataRecipientId, source => source.MapFrom(source => source.DataRecipientId))
                .ForMember(dest => dest.DataRecipientStatus, source => source.MapFrom(source => source.DataRecipientStatus));

            CreateMap<DataRecipientStatusV1[], ResponseRegisterDataRecipientStatusListV1>()
                .ForMember(dest => dest.DataRecipients, source => source.MapFrom(source => source));

            CreateMap<DataRecipientStatus, RegisterDataRecipientStatusModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntityId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<DataRecipientStatus[], ResponseRegisterDataRecipientStatusList>()
                .ForMember(dest => dest.DataRecipients, source => source.MapFrom(source => source));

            // SoftwareProductStatus
            CreateMap<SoftwareProductStatus, RegisterSoftwareProductStatusModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            CreateMap<SoftwareProductStatus, RegisterSoftwareProductStatusModelV1>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.SoftwareProductStatus, source => source.MapFrom(source => source.Status));

            CreateMap<SoftwareProductStatus[], ResponseRegisterSoftwareProductStatusList>()
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source));

            CreateMap<SoftwareProductStatus[], ResponseRegisterSoftwareProductStatusListV1>()
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source));
        }
    }
}
