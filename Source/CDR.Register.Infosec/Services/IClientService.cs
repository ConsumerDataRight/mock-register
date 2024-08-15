using System.Threading.Tasks;
using CDR.Register.Domain.Entities;

namespace CDR.Register.Infosec.Interfaces
{
    public interface IClientService
    {
        Task<SoftwareProductInfosec?> GetClientAsync(string clientId);
    }
}
