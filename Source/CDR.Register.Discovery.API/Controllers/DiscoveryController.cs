using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Discovery.API.Business;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class DiscoveryController : ControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly IDataRecipientStatusCheckService _statusCheckService;

        public DiscoveryController(
            IDiscoveryService discoveryService,
            IDataRecipientStatusCheckService statusCheckService)
        {
            _discoveryService = discoveryService;
            _statusCheckService = statusCheckService;
        }

        [PolicyAuthorize(AuthorisationPolicy.DataHolderBrandsApi)]
        [HttpGet("v1/{industry}/data-holders/brands", Name = "GetDataHolderBrandsLegacy")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataHolderBrandsLegacy(
            string industry,
            [FromQuery(Name = "updated-since"), CheckDate] string updatedSince,
            [FromQuery(Name = "page"), CheckPage] string page,
            [FromQuery(Name = "page-size"), CheckPageSize] string pageSize)
        {
            // Check if the data recipient is active
            var result = await CheckSoftwareProduct();
            if (result != null)
            {
                return result;
            }

            // Set the default values for the incoming parameters
            DateTime? updatedSinceDate = string.IsNullOrEmpty(updatedSince) ? (DateTime?)null : DateTime.Parse(updatedSince);
            int pageNumber = string.IsNullOrEmpty(page) ? 1 : int.Parse(page);
            int pageSizeNumber = string.IsNullOrEmpty(pageSize) ? 25 : int.Parse(pageSize);
            var response = await _discoveryService.GetDataHolderBrandsAsyncV1(industry.ToIndustry(), updatedSinceDate, pageNumber, pageSizeNumber);

            // Check if the given page number is out of range
            if (pageNumber != 1 && pageNumber > response.Meta.TotalPages)
            {
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.PageOutOfRange()));
            }

            // Set pagination meta data
            response.Links = this.GetPaginated("GetDataHolderBrandsLegacy", updatedSinceDate, pageNumber, response.Meta.TotalPages, pageSizeNumber);

            return Ok(response);
        }

        [PolicyAuthorize(AuthorisationPolicy.DataHolderBrandsApiMultiIndustry)]
        [HttpGet("v1/data-holders/brands", Name = "GetDataHolderBrands")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataHolderBrands(
            [FromQuery(Name = "updated-since"), CheckDate] string updatedSince,
            [FromQuery(Name = "page"), CheckPage] string page,
            [FromQuery(Name = "page-size"), CheckPageSize] string pageSize)
        {
            // Check if the data recipient is active
            var result = await CheckSoftwareProduct();
            if (result != null)
            {
                return result;
            }

            // Set the default values for the incoming parameters
            DateTime? updatedSinceDate = string.IsNullOrEmpty(updatedSince) ? (DateTime?)null : DateTime.Parse(updatedSince);
            int pageNumber = string.IsNullOrEmpty(page) ? 1 : int.Parse(page);
            int pageSizeNumber = string.IsNullOrEmpty(pageSize) ? 25 : int.Parse(pageSize);
            var response = await _discoveryService.GetDataHolderBrandsAsync(Industry.ALL, updatedSinceDate, pageNumber, pageSizeNumber);

            // Check if the given page number is out of range
            if (pageNumber != 1 && pageNumber > response.Meta.TotalPages)
            {
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.PageOutOfRange()));
            }

            // Set pagination meta data
            response.Links = this.GetPaginated("GetDataHolderBrands", updatedSinceDate, pageNumber, response.Meta.TotalPages, pageSizeNumber);

            return Ok(response);
        }

        [PolicyAuthorize(AuthorisationPolicy.DataHolderBrandsApiMultiIndustry)]
        [HttpGet("v1/data-holders/brands/{industry}", Name = "GetDataHolderBrandsIndustry")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataHolderBrandsIndustry(
            [FromQuery(Name = "updated-since"), CheckDate] string updatedSince,
            [FromQuery(Name = "page"), CheckPage] string page,
            [FromQuery(Name = "page-size"), CheckPageSize] string pageSize,
            string industry)
        {
            // Check if the data recipient is active
            var result = await CheckSoftwareProduct();
            if (result != null)
            {
                return result;
            }

            // Set the default values for the incoming parameters
            DateTime? updatedSinceDate = string.IsNullOrEmpty(updatedSince) ? (DateTime?)null : DateTime.Parse(updatedSince);
            int pageNumber = string.IsNullOrEmpty(page) ? 1 : int.Parse(page);
            int pageSizeNumber = string.IsNullOrEmpty(pageSize) ? 25 : int.Parse(pageSize);
            var response = await _discoveryService.GetDataHolderBrandsAsync(industry.ToIndustry(), updatedSinceDate, pageNumber, pageSizeNumber);

            // Check if the given page number is out of range
            if (pageNumber != 1 && pageNumber > response.Meta.TotalPages)
            {
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.PageOutOfRange()));
            }

            // Set pagination meta data
            response.Links = this.GetPaginated("GetDataHolderBrandsIndustry", updatedSinceDate, pageNumber, response.Meta.TotalPages, pageSizeNumber);

            return Ok(response);
        }

        /// <remarks>
        /// This x-v version = 1 method is required by the API Version dependency injection pipeline as set in Startup.cs
        /// If NO x-v header is sent in the request the API Version will default to x-v = 1 ie. this method, and it will respond as if a request
        /// was made to highest version of the Legacy endpoint supported.
        /// If this method is removed API Version will throw an exception error as the default endpoint will not be found.
        /// </remarks>
        [Obsolete("This API version has been superseded")]
        [HttpGet]
        [Route("v1/{industry}/data-recipients")]
        [CheckXV("2")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataRecipientsV1(string industry)
        {
            return Ok(await _discoveryService.GetDataRecipientsAsyncV1(Industry.BANKING));
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataRecipientsV2(string industry)
        {
            return Ok(await _discoveryService.GetDataRecipientsAsyncV1(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/data-recipients")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataRecipients()
        {
            return Ok(await _discoveryService.GetDataRecipientsAsync());
        }

        /// <summary>
        /// Performs checks against the software product id from the access token.
        /// </summary>
        /// <returns>An ActionResult if there is an error to return, otherwise null if there are no issues.</returns>
        private async Task<IActionResult> CheckSoftwareProduct()
        {
            // Get the software product id based on the access token.
            var softwareProductIdAsGuid = GetSoftwareProductIdFromAccessToken();
            if (softwareProductIdAsGuid == null)
            {
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.UnknownError()));
            }

            // Check the status of the data recipient making the SSA request.
            var statusErrors = await CheckStatus(softwareProductIdAsGuid.Value);
            if (statusErrors.HasErrors())
            {
                return new RegisterForbidResult(statusErrors);
            }

            return null;
        }

        private Guid? GetSoftwareProductIdFromAccessToken()
        {
            string clientId = User.FindFirst("client_id")?.Value;
            if (Guid.TryParse(clientId, out Guid softwareProductId))
            {
                return softwareProductId;
            }
            return null;
        }

        private async Task<ResponseErrorList> CheckStatus(Guid softwareProductId)
        {
            return await _statusCheckService.ValidateSoftwareProductStatus(softwareProductId);
        }
    }
}