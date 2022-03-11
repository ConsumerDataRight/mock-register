using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.SSA.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace CDR.Register.SSA.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class SSAController : ControllerBase
    {
        private readonly ISSAService _ssaService;
        private readonly ICertificateService _certificateService;
        private readonly IDataRecipientStatusCheckService _statusCheckService;

        public SSAController(
            ISSAService ssaService,
            ICertificateService certificateService,
            IDataRecipientStatusCheckService statusCheckService)
        {
            _ssaService = ssaService;
            _certificateService = certificateService;
            _statusCheckService = statusCheckService;
        }

        /// <remarks>
        /// This x-v version = 1 method is required by the API Version dependency injection pipeline as set in Startup.cs
        /// If NO x-v header is sent in the request the API Version will default to x-v = 1 ie. this method, and it will respond as if a request
        /// was made to highest version of the Legacy endpoint supported.
        /// If this method is removed API Version will throw an exception error as the default endpoint will not be found.
        /// </remarks>
        [Obsolete("This API version has been superseded")]
        [PolicyAuthorize(AuthorisationPolicy.GetSSA)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("2")]
        [ApiVersion("1")]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetSoftwareStatementAssertionV1(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            return await GetSoftwareStatementAssertionJWTV2(dataRecipientBrandId, softwareProductId);
        }

        [PolicyAuthorize(AuthorisationPolicy.GetSSA)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetSoftwareStatementAssertionV2(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            return await GetSoftwareStatementAssertionJWTV2(dataRecipientBrandId, softwareProductId);
        }

        [PolicyAuthorize(AuthorisationPolicy.GetSSAMultiIndustry)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("3")]
        [ApiVersion("3")]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetSoftwareStatementAssertionV3(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            var result = await CheckSoftwareProduct(softwareProductId);
            if (result != null)
                return result;

            var ssa = await _ssaService.GetSoftwareStatementAssertionJWTAsync(dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : Ok(ssa);
        }

        [PolicyAuthorize(AuthorisationPolicy.GetSSAMultiIndustry)]
        [HttpGet]
        [Route("v1/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetSoftwareStatementAssertion(string dataRecipientBrandId, string softwareProductId)
        {
            var result = await CheckSoftwareProduct(softwareProductId);
            if (result != null)
                return result;

            var ssa = await _ssaService.GetSoftwareStatementAssertionJWTAsync(dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : Ok(ssa);
        }

        [HttpGet]
        [Route("v1/jwks")]
        [ApiVersion("1")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public IActionResult GetJwks()
        {
            return Ok(_certificateService.JsonWebKeySet);
        }

        private Guid? GetSoftwareProductIdFromAccessToken()
        {
            string clientId = User.FindFirst("client_id")?.Value;
            if (Guid.TryParse(clientId, out Guid softwareProductId))
                return softwareProductId;

            return null;
        }

        /// <summary>
        /// Performs status check against the softwareProductId parameter
        /// </summary>
        /// <param name="softwareProductId">Software Product ID</param>
        /// <returns>An ActionResult if there is an error to return, otherwise null if there are no issues.</returns>
        private async Task<IActionResult> CheckSoftwareProduct(string softwareProductId)
        {
            // Get the software product id based on the access token.
            var softwareProductIdAsGuid = GetSoftwareProductIdFromAccessToken();
            if (softwareProductIdAsGuid == null)
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.UnknownError()));

            // Ensure that the software product id from the access token matches the software product id in the request.
            if (!softwareProductIdAsGuid.ToString().Equals(softwareProductId, StringComparison.OrdinalIgnoreCase))
                return new NotFoundObjectResult(new ResponseErrorList(ResponseErrorList.InvalidSoftwareProduct(softwareProductId)));

            // Check the status of the data recipient making the SSA request.
            var statusErrors = await CheckStatus(softwareProductIdAsGuid.Value);
            if (statusErrors.HasErrors())
                return new RegisterForbidResult(statusErrors);

            return null;
        }

        private async Task<ResponseErrorList> CheckStatus(Guid softwareProductId)
        {
            return await _statusCheckService.ValidateSoftwareProductStatus(softwareProductId);
        }

        private async Task<IActionResult> GetSoftwareStatementAssertionJWTV2(string dataRecipientBrandId, string softwareProductId)
        {
            var result = await CheckSoftwareProduct(softwareProductId);
            if (result != null)
                return result;

            var ssa = await _ssaService.GetSoftwareStatementAssertionJWTV2Async(dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : Ok(ssa);
        }
    }
}