using System;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Domain.Repositories;

namespace CDR.Register.Discovery.API.Business
{
    public class DiscoveryService : IDiscoveryService
    {
        private readonly IRegisterDiscoveryRepository _registerDiscoveryRepository;
        private readonly IMapper _mapper;

        public DiscoveryService(
            IRegisterDiscoveryRepository registerDiscoveryRepository,
            IMapper mapper)
        {
            _registerDiscoveryRepository = registerDiscoveryRepository;
            _mapper = mapper;
        }

        public async Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsync((Domain.Entities.Industry)industry, updatedSince, page, pageSize);
            var response = _mapper.Map<ResponseRegisterDataHolderBrandList>(entity);

            return response;
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsync((Domain.Entities.Industry)industry);
            var response = _mapper.Map<ResponseRegisterDataRecipientList>(entity);

            return response;
        }

        public async Task<ResponseRegisterDataRecipientListV2> GetDataRecipientsV2Async(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsync((Domain.Entities.Industry)industry);
            var response = _mapper.Map<ResponseRegisterDataRecipientListV2>(entity);

            return response;
        }

    }
}
