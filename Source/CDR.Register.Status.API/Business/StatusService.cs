using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Domain.Repositories;
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
            _registerStatusRepository = registerStatusRepository;
            _mapper = mapper;
        }

        public async Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(Industry industry)
        {
            var entity = await _registerStatusRepository.GetDataRecipientStatusesAsync((Domain.Entities.Industry)industry);

            var response = _mapper.Map<ResponseRegisterDataRecipientStatusList>(entity);
            return response;
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(Industry industry)
        {
            var entity = await _registerStatusRepository.GetSoftwareProductStatusesAsync((Domain.Entities.Industry)industry);

            var response = _mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
            return response;
        }
    }
}
