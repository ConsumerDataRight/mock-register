﻿using System;
using System.Net;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Domain.Models;
using CDR.Register.SSA.API.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CDR.Register.SSA.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class SsaController : ControllerBase
    {
        private readonly ISsaService _ssaService;
        private readonly ICertificateService _certificateService;
        private readonly IDataRecipientStatusCheckService _statusCheckService;
        private readonly IConfiguration _configuration;

        public SsaController(
            ISsaService ssaService,
            ICertificateService certificateService,
            IDataRecipientStatusCheckService statusCheckService,
            IConfiguration configuration)
        {
            this._ssaService = ssaService;
            this._certificateService = certificateService;
            this._statusCheckService = statusCheckService;
            this._configuration = configuration;
        }

        [PolicyAuthorize(RegisterAuthorisationPolicy.GetSSAMultiIndustry)]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/{dataRecipientBrandId}/software-products/{softwareProductId}/ssa")]
        [ReturnXV("3")]
        [ApiVersion("3")]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IActionResult> GetSoftwareStatementAssertionXV3(string industry, string dataRecipientBrandId, string softwareProductId)
        {
            // CTS conformance ID validations
            var basePathExpression = this._configuration.GetValue<string>(Constants.ConfigurationKeys.BasePathExpression);
            if (!string.IsNullOrEmpty(basePathExpression))
            {
                var validIssuer = this.HttpContext.ValidateIssuer();
                if (!validIssuer)
                {
                    return this.Unauthorized(new ResponseErrorList(StatusCodes.Status401Unauthorized.ToString(), HttpStatusCode.Unauthorized.ToString(), "invalid_token"));
                }
            }

            var result = await this.CheckSoftwareProduct(softwareProductId);
            if (result != null)
            {
                return result;
            }

            var ssa = await this._ssaService.GetSoftwareStatementAssertionJWTAsync(industry.ToIndustry(), dataRecipientBrandId, softwareProductId);
            return string.IsNullOrEmpty(ssa) ? this.NotFound(new ResponseErrorList(ResponseErrorList.NotFound())) : this.Ok(ssa);
        }

        [HttpGet]
        [Route("v1/jwks")]
        [ApiVersion("1")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public IActionResult GetJwks()
        {
            return new OkObjectResult(this._certificateService.JsonWebKeySet);
        }

        private Guid? GetSoftwareProductIdFromAccessToken()
        {
            string clientId = this.User.FindFirst("client_id")?.Value;
            if (Guid.TryParse(clientId, out Guid softwareProductId))
            {
                return softwareProductId;
            }

            return null;
        }

        /// <summary>
        /// Performs status check against the softwareProductId parameter.
        /// </summary>
        /// <param name="softwareProductId">Software Product ID.</param>
        /// <returns>An ActionResult if there is an error to return, otherwise null if there are no issues.</returns>
        private async Task<IActionResult> CheckSoftwareProduct(string softwareProductId)
        {
            // Get the software product id based on the access token.
            var softwareProductIdAsGuid = this.GetSoftwareProductIdFromAccessToken();
            if (softwareProductIdAsGuid == null)
            {
                return new BadRequestObjectResult(new ResponseErrorList().AddUnexpectedError());
            }

            // Ensure that the software product id from the access token matches the software product id in the request.
            if (!softwareProductIdAsGuid.ToString().Equals(softwareProductId, StringComparison.OrdinalIgnoreCase))
            {
                return new NotFoundObjectResult(new ResponseErrorList(ResponseErrorList.InvalidSoftwareProduct(softwareProductId)));
            }

            // Check the status of the data recipient making the SSA request.
            var statusErrors = await this.CheckStatus(softwareProductIdAsGuid.Value);
            if (statusErrors.HasErrors())
            {
                return new RegisterForbidResult(statusErrors);
            }

            return null;
        }

        private async Task<ResponseErrorList> CheckStatus(Guid softwareProductId)
        {
            return await this._statusCheckService.ValidateSoftwareProductStatus(softwareProductId);
        }
    }
}
