using AutoMapper;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using System;
using System.Threading.Tasks;

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
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsync(industry, updatedSince, page, pageSize);
            return _mapper.Map<ResponseRegisterDataHolderBrandList>(entity);
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsync(industry);
            return _mapper.Map<ResponseRegisterDataRecipientList>(entity);
        }
    }
}