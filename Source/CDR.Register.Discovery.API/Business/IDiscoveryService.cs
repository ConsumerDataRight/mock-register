﻿using CDR.Register.Discovery.API.Business.Responses;
using CDR.Register.Repository.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Business
{
    public interface IDiscoveryService
    {
        Task<ResponseRegisterDataHolderBrandList> GetDataHolderBrandsAsync(Industry industry, DateTime? updatedSince, int page, int pageSize);
        Task<ResponseRegisterDataRecipientList> GetDataRecipientsAsync(Industry industry);
    }
}
