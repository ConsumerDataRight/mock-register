﻿using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business.Responses;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Business
{
    public interface IStatusService
    {
        Task<ResponseRegisterDataRecipientStatusListV1> GetDataRecipientStatusesAsyncV1();
        Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(IndustryEnum industry);
        Task<ResponseRegisterSoftwareProductStatusListV1> GetSoftwareProductStatusesAsyncV1();
        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(IndustryEnum industry);
        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync();
    }
}