using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.IdentityServer.Interfaces
{
    public interface IJwkService
    {
        Task<IList<JsonWebKey>> GetJwksAsync(string jwksUrl);
    }
}
