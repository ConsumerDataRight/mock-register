using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Interfaces;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.IdentityServer.Extensions
{
    public static class ClientExtensions
    {
        public static async Task<List<SecurityKey>> GetJsonWebKeysAsync(this IEnumerable<Secret> secrets, IJwkService jwkService)
        {
            var secretList = secrets.ToList().AsReadOnly();
            var keys = new List<SecurityKey>();

            foreach (var secret in secretList.Where(s => s.Type == Constants.SecretTypes.JwksUrl))
            {
                var jwks = await jwkService.GetJwksAsync(secret.Value);
                keys.AddRange(jwks);
            }

            return keys;
        }
    }
}
