﻿using CDR.Register.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.Infosec.Interfaces
{
    public interface ITokenService
    {
        Task<(bool IsValid, string? Message, SoftwareProductInfosec? Client)> ValidateClientAssertion(string client_id, string clientAssertion);

        Task<string> CreateAccessToken(
            SoftwareProductInfosec client,
            int expiryInSeconds,
            string scope,
            string cnf);

        Task<IList<SecurityKey>> GetClientKeys(SoftwareProductInfosec client);

        Task<IList<JsonWebKey>> GetClientJwks(SoftwareProductInfosec client);
    }
}
