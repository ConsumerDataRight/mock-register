﻿using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business.Responses;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Business
{
    public interface IStatusService
    {
        Task<ResponseRegisterDataRecipientStatusList> GetDataRecipientStatusesAsync(Industry industry);
        Task<ResponseRegisterSoftwareProductStatusList> GetSoftwareProductStatusesAsync(Industry industry);
        Task<ResponseRegisterDataHolderStatusList> GetDataHolderStatusesAsyncXV1(Industry industry);
    }
}