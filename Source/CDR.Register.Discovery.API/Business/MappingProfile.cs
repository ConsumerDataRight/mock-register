﻿using System.Collections.Generic;
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
            this.CreateMap(typeof(Page<>), typeof(MetaPaginated));

            this.CreateMap<DataHolderAuthentication, AuthDetailModel>();

            this.CreateMap<DataHolderBrandServiceEndpoint, EndpointDetailModel>();

            this.CreateMap<DataHolderLegalEntity, DataHolderLegalEntityModel>()
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

            this.CreateMap<Page<DataHolderBrand[]>, ResponseRegisterDataHolderBrandList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source.Data))
                .ForMember(dest => dest.Meta, source => source.MapFrom(source => source));

            this.CreateMap<DataHolderBrand, RegisterDataHolderBrandModel>()
                .ForMember(dest => dest.DataHolderBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.Industries, source => source.MapFrom(source => new List<string> { source.DataHolder.Industry.ToLower() }))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.AuthDetails, source => source.MapFrom(source => source.DataHolderAuthentications))
                .ForMember(dest => dest.EndpointDetail, source => source.MapFrom(source => source.DataHolderBrandServiceEndpoint))
                .ForMember(dest => dest.LegalEntity, source => source.MapFrom(source => source.DataHolder.LegalEntity));

            this.CreateMap<DataRecipientBrand, DataRecipientBrandModel>()
                .ForMember(dest => dest.DataRecipientBrandId, source => source.MapFrom(source => source.BrandId))
                .ForMember(dest => dest.BrandName, source => source.MapFrom(source => source.BrandName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.BrandStatus))
                .ForMember(dest => dest.SoftwareProducts, source => source.MapFrom(source => source.SoftwareProducts));

            this.CreateMap<DataRecipient[], ResponseRegisterDataRecipientList>()
                .ForMember(dest => dest.Data, source => source.MapFrom(source => source));

            this.CreateMap<DataRecipient, RegisterDataRecipientModel>()
                .ForMember(dest => dest.LegalEntityId, source => source.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(dest => dest.LegalEntityName, source => source.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(dest => dest.AccreditationNumber, source => source.MapFrom(source => source.LegalEntity.AccreditationNumber))
                .ForMember(dest => dest.AccreditationLevel, source => source.MapFrom(source => source.LegalEntity.AccreditationLevelId.ToUpper()))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LegalEntity.LogoUri))
                .ForMember(dest => dest.DataRecipientBrands, source => source.MapFrom(source => source.DataRecipientBrands))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status))
                .ForMember(dest => dest.LastUpdated, source => source.MapFrom(source => source.LastUpdated));

            this.CreateMap<SoftwareProduct, SoftwareProductModel>()
                .ForMember(dest => dest.SoftwareProductId, source => source.MapFrom(source => source.SoftwareProductId))
                .ForMember(dest => dest.SoftwareProductName, source => source.MapFrom(source => source.SoftwareProductName))
                .ForMember(dest => dest.LogoUri, source => source.MapFrom(source => source.LogoUri))
                .ForMember(dest => dest.Status, source => source.MapFrom(source => source.Status));
        }
    }
}
