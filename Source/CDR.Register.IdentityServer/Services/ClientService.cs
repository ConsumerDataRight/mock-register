using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.IdentityServer.Interfaces;
using Microsoft.Extensions.Logging;

namespace CDR.Register.IdentityServer.Services
{
    public class ClientService : IClientService
    {
        private readonly IRegisterIdSvrRepository _idSvrRepository;

        public ClientService(IRegisterIdSvrRepository idSvrRepository)
        {
            _idSvrRepository = idSvrRepository;
        }

        public async Task<SoftwareProductIdSvr> GetClientAsync(string clientId)
        {
            Guid softwareProductId;
            if (!Guid.TryParse(clientId, out softwareProductId))
            {
                return null;
            }

            var softwareProduct = await _idSvrRepository.GetSoftwareProductAsync(softwareProductId);
            return softwareProduct;
        }
    }
}
