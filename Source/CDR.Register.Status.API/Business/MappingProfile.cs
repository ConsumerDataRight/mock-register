﻿using AutoMapper;
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
            this.CreateMap<DataRecipientStatus, RegisterDataRecipientStatusModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.DataRecipientId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            this.CreateMap<DataRecipientStatus[], ResponseRegisterDataRecipientStatusList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // SoftwareProductStatus
            this.CreateMap<SoftwareProductStatus, RegisterSoftwareProductStatusModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            this.CreateMap<SoftwareProductStatus[], ResponseRegisterSoftwareProductStatusList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            // DataHolderStatus
            this.CreateMap<DataHolderStatus, RegisterDataHolderStatusModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntityId))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));

            this.CreateMap<DataHolderStatus[], ResponseRegisterDataHolderStatusList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));
        }
    }
}
