using System;
using System.Threading.Tasks;
using CDR.Register.Domain.Models;
using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.API.Infrastructure.Services
{
    public interface IDataRecipientStatusCheckService
    {
        Task<ResponseErrorList> ValidateSoftwareProductStatus(Guid softwareProductId);
    }
}
