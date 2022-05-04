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

        public async Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsyncXV1(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsyncXV1(industry, updatedSince, page, pageSize);
            return _mapper.Map<ResponseRegisterDataHolderBrandList>(entity);
        }

        public async Task<ResponseRegisterDataHolderBrandListV2> GetDataHolderBrandsAsyncXV2(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsyncXV2(industry, updatedSince, page, pageSize);
            return _mapper.Map<ResponseRegisterDataHolderBrandListV2>(entity);
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsyncXV1(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsyncXV1(industry);
            return _mapper.Map<ResponseRegisterDataRecipientList>(entity);
        }

        public async Task<ResponseRegisterDataRecipientListV2> GetDataRecipientsAsyncXV2(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsyncXV2(industry);
            return _mapper.Map<ResponseRegisterDataRecipientListV2>(entity);
        }

        public async Task<ResponseRegisterDataRecipientListV3> GetDataRecipientsAsyncXV3(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsyncXV3(industry);
            return _mapper.Map<ResponseRegisterDataRecipientListV3>(entity);
        }
    }
}