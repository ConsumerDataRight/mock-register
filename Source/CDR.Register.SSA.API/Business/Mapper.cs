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
            this._config = config;

            var configuration = new MapperConfiguration(cfg =>
            {
                // Base mapping.
                cfg.CreateMap<SoftwareStatementAssertion, SoftwareStatementAssertionModel>()
                .ForMember(d => d.Client_name, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductName))
                .ForMember(d => d.Client_description, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductDescription))
                .ForMember(d => d.Client_uri, s => s.MapFrom(source => source.SoftwareProduct.ClientUri))
                .ForMember(d => d.Jwks_uri, s => s.MapFrom(source => source.SoftwareProduct.JwksUri))
                .ForMember(d => d.Logo_uri, s => s.MapFrom(source => source.SoftwareProduct.LogoUri))
                .ForMember(d => d.Org_id, s => s.MapFrom(source => source.DataRecipientBrand.BrandId))
                .ForMember(d => d.Org_name, s => s.MapFrom(source => source.DataRecipientBrand.BrandName))
                .ForMember(d => d.Policy_uri, s => s.MapFrom(source => source.SoftwareProduct.PolicyUri))
                .ForMember(d => d.Redirect_uris, s => s.MapFrom(source => source.SoftwareProduct.RedirectUris))
                .ForMember(d => d.Revocation_uri, s => s.MapFrom(source => source.SoftwareProduct.RevocationUri))
                .ForMember(d => d.Recipient_base_uri, s => s.MapFrom(source => source.SoftwareProduct.RecipientBaseUri))
                .ForMember(d => d.Scope, s => s.MapFrom(source => source.SoftwareProduct.Scope))
                .ForMember(d => d.Software_roles, s => s.MapFrom(source => "data-recipient-software-product"))
                .ForMember(d => d.Software_id, s => s.MapFrom(source => source.SoftwareProduct.SoftwareProductId))
                .ForMember(d => d.Tos_uri, s => s.MapFrom(source => source.SoftwareProduct.TosUri))
                .ForMember(d => d.Iss, s => s.MapFrom(source => this._config["SSA:Issuer"]))
                .ForMember(d => d.Iat, s => s.MapFrom(source => (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds))
                .ForMember(d => d.Jti, s => s.MapFrom(source => Guid.NewGuid().ToString().Replace("-", string.Empty)))
                .ForMember(d => d.Exp, s => s.MapFrom(source => (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds + long.Parse(this._config["SSA:ExpiryInSeconds"])))
                .ForMember(d => d.Legal_entity_id, s => s.MapFrom(source => source.LegalEntity.LegalEntityId))
                .ForMember(d => d.Legal_entity_name, s => s.MapFrom(source => source.LegalEntity.LegalEntityName))
                .ForMember(d => d.Sector_identifier_uri, s => s.MapFrom(source => source.SoftwareProduct.SectorIdentifierUri));
            });

            this._mapper = configuration.CreateMapper();
        }

        public SoftwareStatementAssertionModel MapV3(SoftwareStatementAssertion softwareStatementAssertion)
        {
            if (softwareStatementAssertion == null)
            {
                return null;
            }

            return this._mapper.Map<SoftwareStatementAssertion, SoftwareStatementAssertionModel>(softwareStatementAssertion);
        }
    }
}
