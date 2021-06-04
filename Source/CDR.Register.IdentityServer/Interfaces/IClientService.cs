using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.IdentityServer.Interfaces
{
    public interface IClientService
    {
        Task<SoftwareProductIdSvr> GetClientAsync(string clientId);
    }
}
