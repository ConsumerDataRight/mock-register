using System;
using Microsoft.AspNetCore.Authorization;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class DataRecipientSoftwareProductIdRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }

        public DataRecipientSoftwareProductIdRequirement(string issuer)
        {
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }
}
