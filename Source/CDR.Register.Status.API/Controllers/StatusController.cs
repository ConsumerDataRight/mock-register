using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IStatusService StatusService, ILogger<StatusController> logger)
        {
            _statusService = StatusService;
            _logger = logger;
        }

        [Obsolete]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(IndustryEnum.BANKING)]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatusV1(string industry)
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetDataRecipientStatusesAsyncV1());
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatus(string industry)
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetDataRecipientStatusesAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/data-recipients/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatusV2()
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetDataRecipientStatusesAsync(IndustryEnum.ALL));
        }

        [Obsolete]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(IndustryEnum.BANKING)]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatusV1(string industry)
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetSoftwareProductStatusesAsyncV1());
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatus(string industry)
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetSoftwareProductStatusesAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/data-recipients/brands/software-products/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatusNoInd()
        {
            using (LogContext.PushProperty("MethodName", ControllerContext.RouteData.Values["action"].ToString()))
            {
                _logger.LogInformation($"Received request to {ControllerContext.RouteData.Values["action"]}");
            }

            return Ok(await _statusService.GetSoftwareProductStatusesAsync());
        }
    }
}