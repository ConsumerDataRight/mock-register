using System;
using Microsoft.AspNetCore.Authorization;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class DataRecipientSoftwareProductIdRequirement : IAuthorizationRequirement
    {
        public DataRecipientSoftwareProductIdRequirement(string issuer)
        {
            this.Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }

        public string Issuer { get; }
    }
}
