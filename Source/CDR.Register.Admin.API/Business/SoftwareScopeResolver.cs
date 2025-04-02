using AutoMapper;
using Microsoft.Extensions.Configuration;
using DomainEntities = CDR.Register.Domain.Entities;

namespace CDR.Register.Admin.API.Business
{
    public class SoftwareScopeResolver : IMemberValueResolver<Model.SoftwareProduct, DomainEntities.SoftwareProduct, string, string>
    {
        private readonly IConfiguration config;

        public SoftwareScopeResolver(IConfiguration config)
        {
            this.config = config;
        }

        public string Resolve(Model.SoftwareProduct source, DomainEntities.SoftwareProduct destination, string sourceMember, string destMember, ResolutionContext context)
        {
            if (source.Scope == null)
            {
                return config["SoftwareProductDefaultScopes"] ?? string.Empty;
            }

            return source.Scope;
        }
    }
}
