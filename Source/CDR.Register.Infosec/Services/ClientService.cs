using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Infosec.Interfaces;

namespace CDR.Register.Infosec.Services
{
    public class ClientService : IClientService
    {
        private readonly IRegisterInfosecRepository _infosecRepository;

        public ClientService(IRegisterInfosecRepository infosecRepository)
        {
            _infosecRepository = infosecRepository;
        }

        public async Task<SoftwareProductInfosec?> GetClientAsync(string? clientId)
        {
            if (clientId == null)
            {
                return null;
            }
            if (!Guid.TryParse(clientId, out Guid softwareProductId))
            {
                return null;
            }

            var softwareProduct = await _infosecRepository.GetSoftwareProductAsync(softwareProductId);
            return softwareProduct;
        }
    }
}
