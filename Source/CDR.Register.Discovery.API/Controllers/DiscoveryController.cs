using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Discovery.API.Business;
using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CDR.Register.Discovery.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class DiscoveryController : ControllerBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly IDataRecipientStatusCheckService _statusCheckService;
        private readonly IConfiguration _configuration;

        // Route names.
        private const string ROUTE_GET_DATA_HOLDER_BRANDS_XV2 = "GetDataHolderBrandsXV2";
        private const string ROUTE_GET_DATA_RECIPIENTS_XV3 = "GetDataRecipientsXV3";

        public DiscoveryController(
            IDiscoveryService discoveryService,
            IConfiguration configuration,
            IDataRecipientStatusCheckService statusCheckService)
        {
            _discoveryService = discoveryService;
            _configuration = configuration;
            _statusCheckService = statusCheckService;
        }


        [HttpGet("v1/{industry}/data-holders/brands", Name = ROUTE_GET_DATA_HOLDER_BRANDS_XV2)]
        [PolicyAuthorize(RegisterAuthorisationPolicy.DataHolderBrandsApiMultiIndustry)]
        [ApiVersion("2")]
        [ReturnXV("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataHolderBrandsXV2(
            string industry,
            [FromQuery(Name = "updated-since"), CheckDate] string updatedSince,
            [FromQuery(Name = "page"), CheckPage] string page,
            [FromQuery(Name = "page-size"), CheckPageSize] string pageSize)
        {

            // CTS conformance ID validations
            var basePathExpression = _configuration.GetValue<string>(Constants.ConfigurationKeys.BasePathExpression);
            if (!string.IsNullOrEmpty(basePathExpression))
            {
                var validIssuer = HttpContext.ValidateIssuer();
                if (!validIssuer)
                {
                    return Unauthorized(new ResponseErrorList(StatusCodes.Status401Unauthorized.ToString(), HttpStatusCode.Unauthorized.ToString(), "invalid_token"));
                }
            }

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
            response.Links = this.GetPaginated(ROUTE_GET_DATA_HOLDER_BRANDS_XV2, _configuration,
                updatedSinceDate, pageNumber, response.Meta.TotalPages.Value, pageSizeNumber, "", true);

            return Ok(response);
        }

        [HttpGet("v1/{industry}/data-recipients", Name = ROUTE_GET_DATA_RECIPIENTS_XV3)]
        [ReturnXV("3")]
        [ApiVersion("3")]
        [ETag]
        [CheckIndustry()]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetDataRecipientsXV3(string industry)
        {
            var response = await _discoveryService.GetDataRecipientsAsync(industry.ToIndustry());
            response.Links = this.GetSelf(_configuration, HttpContext, "");
            return Ok(response);
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
                return new BadRequestObjectResult(new ResponseErrorList().AddUnexpectedError());
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