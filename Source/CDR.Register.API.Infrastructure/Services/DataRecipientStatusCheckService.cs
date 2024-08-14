using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Models;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;

namespace CDR.Register.API.Infrastructure.Services
{

    /// <summary>
    /// Service to check the status of a Data Recipient.
    /// </summary>
    /// <remarks>
    /// Checks the:
    /// - software product status = ACTIVE
    /// - brand status = ACTIVE
    /// - participation status = ACTIVE
    /// </remarks>
    public class DataRecipientStatusCheckService : IDataRecipientStatusCheckService
    {
        private readonly IRegisterDiscoveryRepository _registerDiscoveryRepository;

        public DataRecipientStatusCheckService(IRegisterDiscoveryRepository registerDiscoveryRepository)
        {
            _registerDiscoveryRepository = registerDiscoveryRepository;
        }

        public async Task<ResponseErrorList> ValidateSoftwareProductStatus(Guid softwareProductId)
        {
            // Get the latest data recipient details from the repository
            var softwareProduct = await _registerDiscoveryRepository.GetSoftwareProductIdAsync(softwareProductId);

            // Perform validations
            ResponseErrorList errorList = new ResponseErrorList();
            if (softwareProduct == null)
            {
                errorList.Errors.Add(ResponseErrorList.DataRecipientSoftwareProductNotActive());
                return errorList;
            }

            if (!softwareProduct.IsActive)
            {
                errorList.Errors.Add(ResponseErrorList.DataRecipientSoftwareProductNotActive());
            }
            if (!softwareProduct.DataRecipientBrand.IsActive || !softwareProduct.DataRecipientBrand.DataRecipient.IsActive)
            {
                errorList.Errors.Add(ResponseErrorList.DataRecipientParticipationNotActive());
            }

            return errorList;
        }
    }
}
