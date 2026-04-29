using System;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Repository.Specifications;
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

        public async Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatuses(Industry industry, int version)
        {
            IParticipationSpecification specification = version switch
            {
                2 => new ParticipationsSpecifications.ExcludeNblIndustry(),
                3 => new ParticipationsSpecifications.AllIndustries(),
                _ => throw new NotImplementedException("Unknown version"),
            };

            var entity = await this._registerStatusRepository.GetDataRecipientStatuses(industry, specification);
            return this._mapper.Map<ResponseRegisterDataRecipientStatusList>(entity);
        }

        public async Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatuses(Industry industry, int version)
        {
            IParticipationSpecification specification = version switch
            {
                2 => new ParticipationsSpecifications.ExcludeNblIndustry(),
                3 => new ParticipationsSpecifications.AllIndustries(),
                _ => throw new NotImplementedException("Unknown version"),
            };

            var entity = await this._registerStatusRepository.GetSoftwareProductStatuses(industry, specification);
            return this._mapper.Map<ResponseRegisterSoftwareProductStatusList>(entity);
        }

        public async Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatuses(Industry industry, int version)
        {
            IParticipationSpecification specification = version switch
            {
                1 => new ParticipationsSpecifications.ExcludeNblIndustry(),
                2 => new ParticipationsSpecifications.AllIndustries(),
                _ => throw new NotImplementedException("Unknown version"),
            };

            var entity = await this._registerStatusRepository.GetDataHolderStatuses(industry, specification);
            return this._mapper.Map<ResponseRegisterDataHolderStatusList>(entity);
        }
    }
}
