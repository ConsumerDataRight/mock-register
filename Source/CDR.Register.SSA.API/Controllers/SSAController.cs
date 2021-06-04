using System;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.SSA.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CDR.Register.SSA.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class SSAController : ControllerBase
    {
        private readonly ISSAService _ssaService;
        private readonly ILogger<SSAController> _logger;
        private readonly ICertificateService _certificateService;
        private readonly IDataRecipientStatusCheckService _statusCheckService;

        public SSAController(
            ISSAService ssaService,
            ILogger<SSAController> logger,
            ICertificateService certificateService,
            IDataRecipientStatusCheckService statusCheckService)
        {
            _ssaService = ssaService;
            _logger = logger;
            _certificateService = certificateService;
            _statusCheckService = statusCheckService;
        }

        [PolicyAuthorize(AuthorisationPolicy.GetSoftwareStatementAssertion)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("1")]
        [CheckIndustry]
        [ApiVersion("1")]
        public async Task<IActionResult> GetSoftwareStatementAssertion(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            _logger.LogInformation($"Request received to {nameof(SSAController)}.{nameof(GetSoftwareStatementAssertion)}");

            var result = await CheckSoftwareProduct(softwareProductId);
            if (result != null)
            {
                return result;
            }

            var ssa = await _ssaService.GetSoftwareStatementAssertionJWTAsync(dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : Ok(ssa);
        }

        [PolicyAuthorize(AuthorisationPolicy.GetSoftwareStatementAssertion)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [CheckXV("2")]
        [CheckIndustry]
        [ApiVersion("2")]
        public async Task<IActionResult> GetSoftwareStatementAssertionV2(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            _logger.LogInformation($"Request received to {nameof(SSAController)}.{nameof(GetSoftwareStatementAssertionV2)}");

            var result = await CheckSoftwareProduct(softwareProductId);
            if (result != null)
            {
                return result;
            }

            var ssa = await _ssaService.GetSoftwareStatementAssertionJWTV2Async(dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : Ok(ssa);
        }

        [HttpGet]
        [Route("v1/jwks")]
        [ApiVersion("1")]
        public async Task<IActionResult> GetJwks()
        {
            _logger.LogInformation($"Request received to {nameof(SSAController)}.{nameof(GetJwks)}");

            return Ok(_certificateService.JsonWebKeySet);
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
        /// Performs checks against the softwareProductId parameter.
        /// </summary>
        /// <param name="softwareProductId">Software Product ID</param>
        /// <returns>An ActionResult if there is an error to return, otherwise null if there are no issues.</returns>
        private async Task<IActionResult> CheckSoftwareProduct(string softwareProductId)
        {
            // Get the software product id based on the access token.
            var softwareProductIdAsGuid = GetSoftwareProductIdFromAccessToken();
            if (softwareProductIdAsGuid == null)
            {
                return new BadRequestObjectResult(new ResponseErrorList(ResponseErrorList.UnknownError()));
            }

            // Ensure that the software product id from the access token matches the software product id in the request.
            if (!softwareProductIdAsGuid.ToString().Equals(softwareProductId, StringComparison.OrdinalIgnoreCase))
            {
                return new NotFoundObjectResult(new ResponseErrorList(ResponseErrorList.InvalidSoftwareProduct()));
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
