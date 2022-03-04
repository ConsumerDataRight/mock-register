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

        public async Task<ResponseRegisterDataRecipientStatusListV1> GetDataRecipientStatusesAsyncV1()
        {
            var entity = await _registerStatusRepository.GetDataRecipientStatusesAsyncV1();
            var response = _mapper.Map<ResponseRegisterDataRecipientStatusListV1>(entity);
            return response;
        }

        public async Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(IndustryEnum industry)
        {
            var entity = await _registerStatusRepository.GetDataRecipientStatusesAsync(industry);
            var response = _mapper.Map<ResponseRegisterDataRecipientStatusList>(entity);
            return response;
        }

        public async Task<ResponseRegisterSoftwareProductStatusListV1> GetSoftwareProductStatusesAsyncV1()
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsyncV1();
            var response = _mapper.Map<ResponseRegisterSoftwareProductStatusListV1>(entity);
            return response;
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(IndustryEnum industry)
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsync(industry);
            var response = _mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
            return response;
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync()
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsync();
            var response = _mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
            return response;
        }
    }
}