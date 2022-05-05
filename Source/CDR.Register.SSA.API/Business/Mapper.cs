using System;
using AutoMapper;
using CDR.Register.Domain.Entities;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.Extensions.Configuration;

namespace CDR.Register.SSA.API.Business
{
    public class Mapper : IMapper
    {
        private readonly IConfiguration _config;
        private readonly AutoMapper.IMapper _mapper;

        public Mapper(IConfiguration config)
        {
            _config = config;

            var configuration = new MapperConfiguration(cfg =>
            {
                // Base mapping.
                cfg.CreateMap<SoftwareStatementAssertion, SoftwareStatementAssertionModel>()
                .ForMember(d => d.client_name, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductName))
                .ForMember(d => d.client_description, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductDescription))
                .ForMember(d => d.client_uri, s => s.MapFrom(source => source.SoftwareProduct.ClientUri))
                .ForMember(d => d.jwks_uri, s => s.MapFrom(source => source.SoftwareProduct.JwksUri))
                .ForMember(d => d.logo_uri, s => s.MapFrom(source => source.SoftwareProduct.LogoUri))
                .ForMember(d => d.org_id, s => s.MapFrom(source => source.DataRecipientBrand.BrandId))
                .ForMember(d => d.org_name, s => s.MapFrom(source => source.DataRecipientBrand.BrandName))
                .ForMember(d => d.policy_uri, s => s.MapFrom(source => source.SoftwareProduct.PolicyUri))
                .ForMember(d => d.redirect_uris, s => s.MapFrom(source => source.SoftwareProduct.RedirectUris))
                .ForMember(d => d.revocation_uri, s => s.MapFrom(source => source.SoftwareProduct.RevocationUri))
                .ForMember(d => d.recipient_base_uri, s => s.MapFrom(source => source.SoftwareProduct.RecipientBaseUri))
                .ForMember(d => d.scope, s => s.MapFrom(source => source.SoftwareProduct.Scope))
                .ForMember(d => d.software_roles, s => s.MapFrom(source => "data-recipient-software-product"))
                .ForMember(d => d.software_id, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductId))
                .ForMember(d => d.tos_uri, s => s.MapFrom(source => source.SoftwareProduct.TosUri))
                .ForMember(d => d.iss, s => s.MapFrom(source => _config["SSA:Issuer"]))
                .ForMember(d => d.iat, s => s.MapFrom(source => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds))
                .ForMember(d => d.jti, s => s.MapFrom(source => Guid.NewGuid().ToString().Replace("-", string.Empty)))
                .ForMember(d => d.exp, s => s.MapFrom(source => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + long.Parse(_config["SSA:ExpiryInSeconds"])));

                // Additional V2 mapping.
                cfg.CreateMap<SoftwareStatementAssertion, SoftwareStatementAssertionModelV2>()
                .IncludeBase<SoftwareStatementAssertion, SoftwareStatementAssertionModel>()
                .ForMember(d => d.legal_entity_id, s => s.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(d => d.legal_entity_name, s => s.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(d => d.sector_identifier_uri, s => s.MapFrom(source => source.SoftwareProduct.SectorIdentifierUri));

                // Additional V3 mapping.
                cfg.CreateMap<SoftwareStatementAssertion, SoftwareStatementAssertionModelV3>()
                .IncludeBase<SoftwareStatementAssertion, SoftwareStatementAssertionModelV2>();
            });

            _mapper = configuration.CreateMapper();
        }

        public SoftwareStatementAssertionModel Map(SoftwareStatementAssertion softwareStatementAssertion)
        {
            if (softwareStatementAssertion == null)
            {
                return null;
            }
            return _mapper.Map<SoftwareStatementAssertion, SoftwareStatementAssertionModel>(softwareStatementAssertion);
        }

        public SoftwareStatementAssertionModelV2 MapV2(SoftwareStatementAssertion softwareStatementAssertion)
        {
            if (softwareStatementAssertion == null)
            {
                return null;
            }
            return _mapper.Map<SoftwareStatementAssertion, SoftwareStatementAssertionModelV2>(softwareStatementAssertion);
        }

        public SoftwareStatementAssertionModelV3 MapV3(SoftwareStatementAssertion softwareStatementAssertion)
        {
            if (softwareStatementAssertion == null)
            {
                return null;
            }
            return _mapper.Map<SoftwareStatementAssertion, SoftwareStatementAssertionModelV3>(softwareStatementAssertion);
        }
    }
}
