using AutoMapper;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Status.API.Business.Responses;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Business
{
    public class StatusService : IStatusService
    {
        private readonly IRegisterStatusRepository _registerStatusRepository;
        private readonly IMapper _mapper;

        public StatusService(
            IRegisterStatusRepository registerStatusRepository,
            IMapper mapper)
        {
            _registerStatusRepository = registerStatusRepository;
            _mapper = mapper;
        }

        public async Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsyncXV1(Industry industry)
        {
            var entity = await _registerStatusRepository.GetDataRecipientStatusesAsync(industry);
            return _mapper.Map<ResponseRegisterDataRecipientStatusList>(entity);
        }

        public async Task<ResponseRegisterDataRecipientStatusListV2> GetDataRecipientStatusesAsyncXV2(Industry industry)
        {
            var entity = await _registerStatusRepository.GetDataRecipientStatusesAsync(industry);
            return _mapper.Map<ResponseRegisterDataRecipientStatusListV2>(entity);
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsyncXV1(Industry industry)
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsync(industry);
            return _mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
        }

        public async Task<ResponseRegisterSoftwareProductStatusListV2> GetSoftwareProductStatusesAsyncXV2(Industry industry)
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsync(industry);
            return _mapper.Map<ResponseRegisterSoftwareProductStatusListV2>(entity);
        }

        public async Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatusesAsyncXV1(Industry industry)
        {
            var entity = await _registerStatusRepository.GetDataHolderStatusesAsync(industry);
            return _mapper.Map<ResponseRegisterDataHolderStatusList>(entity);
        }
    }
}