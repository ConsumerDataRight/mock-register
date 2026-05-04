using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Status.API.Business.Responses;

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
            this._registerStatusRepository = registerStatusRepository;
            this._mapper = mapper;
        }

        public async Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(Industry industry)
        {
            var entity = await this._registerStatusRepository.GetDataRecipientStatusesAsync(industry);
            return this._mapper.Map<ResponseRegisterDataRecipientStatusList>(entity);
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(Industry industry)
        {
            var entity = await this._registerStatusRepository.GetSoftwareProductStatusesAsync(industry);
            return this._mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
        }

        public async Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatusesAsyncXV1(Industry industry)
        {
            var entity = await this._registerStatusRepository.GetDataHolderStatusesAsync(industry);
            return this._mapper.Map<ResponseRegisterDataHolderStatusList>(entity);
        }
    }
}
