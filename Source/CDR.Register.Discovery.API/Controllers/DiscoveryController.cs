using System;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Discovery.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CDR.Register.Discovery.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    [CheckIndustry]
    public class DiscoveryController : ControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly ILogger<DiscoveryController> _logger;
        private readonly IDataRecipientStatusCheckService _statusCheckService;

        public DiscoveryController(IDiscoveryService discoveryService, ILogger<DiscoveryController> logger, IDataRecipientStatusCheckService statusCheckService)
        {
            _discoveryService = discoveryService;
            _logger = logger;
            _statusCheckService = statusCheckService;
        }

        [PolicyAuthorize(AuthorisationPolicy.DataHolderBrandsApi)]
        [HttpGet("v1/{industry}/data-holders/brands", Name = "GetDataHolderBrands")]
        [CheckXV("1")]
        [ApiVersion("1")]
        public async Task<IActionResult> GetDataHolderBrands(
            string industry,
            [FromQuery(Name = "updated-since"), CheckDate] string updatedSince,
            [FromQuery(Name = "page"), CheckPage] string page,
            [FromQuery(Name = "page-size"), CheckPageSize] string pageSize)
        {
            _logger.LogInformation($"Request received to {nameof(DiscoveryController)}.{nameof(GetDataHolderBrands)}");

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
            response.Links = this.GetPaginated("GetDataHolderBrands", updatedSinceDate, pageNumber, response.Meta.TotalPages, pageSizeNumber);

            return Ok(response);
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        public async Task<IActionResult> GetDataRecipients(string industry)
        {
            _logger.LogInformation($"Request received to {nameof(DiscoveryController)}.{nameof(GetDataRecipients)}");
            return Ok(await _discoveryService.GetDataRecipientsAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        public async Task<IActionResult> GetDataRecipientsV2(string industry)
        {
            _logger.LogInformation($"Request received to {nameof(DiscoveryController)}.{nameof(GetDataRecipientsV2)}");
            return Ok(await _discoveryService.GetDataRecipientsV2Async(industry.ToIndustry()));
        }

        private Guid? GetSoftwareProductIdFromAccessToken()
        {
            string clientId = User.FindFirst("client_id")?.Value;
            Guid softwareProductId;
            if (Guid.TryParse(clientId, out softwareProductId))
            {
                return softwareProductId;
            }
            return null;
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

        private async Task<ResponseErrorList> CheckStatus(Guid softwareProductId)
        {
            return await _statusCheckService.ValidateDataRecipientStatus(softwareProductId);
        }
    }
}
