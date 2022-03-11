using AutoMapper;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<ResponseRegisterDataHolderBrandListV1> GetDataHolderBrandsAsyncV1(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsyncV1(industry, updatedSince, page, pageSize);
            var response = _mapper.Map<ResponseRegisterDataHolderBrandListV1>(entity);

            return response;
        }

        public async Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await _registerDiscoveryRepository.GetDataHolderBrandsAsync(industry, updatedSince, page, pageSize);
            var response = _mapper.Map<ResponseRegisterDataHolderBrandList>(entity);

            return response;
        }

        public async Task<ResponseRegisterDataRecipientListV1> GetDataRecipientsAsyncV1(Industry industry)
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsyncV1(industry);
            var response = _mapper.Map<ResponseRegisterDataRecipientListV1>(entity);

            return response;
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync()
        {
            var entity = await _registerDiscoveryRepository.GetDataRecipientsAsync();
            var response = _mapper.Map<ResponseRegisterDataRecipientList>(entity);

            return response;
        }
    }
}