using System;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;

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
            this._registerDiscoveryRepository = registerDiscoveryRepository;
            this._mapper = mapper;
        }

        public async Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize)
        {
            var entity = await this._registerDiscoveryRepository.GetDataHolderBrandsAsync(industry, updatedSince, page, pageSize);
            return this._mapper.Map<ResponseRegisterDataHolderBrandList>(entity);
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync(Industry industry)
        {
            var entity = await this._registerDiscoveryRepository.GetDataRecipientsAsync(industry);
            return this._mapper.Map<ResponseRegisterDataRecipientList>(entity);
        }
    }
}
