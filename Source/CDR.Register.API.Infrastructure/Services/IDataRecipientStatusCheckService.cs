using System;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.API.Infrastructure.Services
{
    public interface IDataRecipientStatusCheckService
    {
        Task<ResponseErrorList> ValidateSoftwareProductStatusByIndustry(IndustryEnum industry, Guid softwareProductId);
        Task<ResponseErrorList> ValidateSoftwareProductStatus(Guid softwareProductId);
    }
}
