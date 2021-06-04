using System;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Models;

namespace CDR.Register.API.Infrastructure.Services
{
    public interface IDataRecipientStatusCheckService
    {
        Task<ResponseErrorList> ValidateDataRecipientStatus(Guid softwareProductId);
    }
}
