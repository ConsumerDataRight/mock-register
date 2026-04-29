using System;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Discovery.API.Business.Models;
using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Interfaces;
using CDR.Register.Repository.Specifications;

namespace CDR.Register.Discovery.API.Business
{
    public class DiscoveryService(IRegisterDiscoveryRepository registerDiscoveryRepository, IMapper mapper) : IDiscoveryService
    {
        public async Task<IResponseRegisterDataHolderBrandList<IRegisterDataHolderBrand>> GetDataHolderBrands(Industry industry, DateTime? updatedSince, int page, int pageSize, int version)
        {
            IBrandSpecification specification = version switch
            {
                2 => new BrandSpecifications.ExcludeNblIndustry(),
                3 => new BrandSpecifications.AllIndustries(),
                _ => throw new NotImplementedException("Unknown version"),
            };

            var entity = await registerDiscoveryRepository.GetDataHolderBrands(industry, specification, updatedSince, page, pageSize);

            return version switch
            {
                2 => mapper.Map<ResponseRegisterDataHolderBrandList>(entity),
                3 => mapper.Map<ResponseRegisterDataHolderBrandListV2>(entity),
                _ => throw new NotImplementedException("Unknown version"),
            };
        }

        public async Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync()
        {
            var entity = await registerDiscoveryRepository.GetDataRecipientsAsync();
            return mapper.Map<ResponseRegisterDataRecipientList>(entity);
        }
    }
}
