using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Status.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CDR.Register.Status.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;

        public StatusController(IStatusService StatusService)
        {
            _statusService = StatusService;
        }

        [Obsolete("This API version has been superseded")]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatusV1(string industry)
        {
            return Ok(await _statusService.GetDataRecipientStatusesAsyncV1());
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatus(string industry)
        {
            return Ok(await _statusService.GetDataRecipientStatusesAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/data-recipients/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatusV2()
        {
            return Ok(await _statusService.GetDataRecipientStatusesAsync(Industry.ALL));
        }

        [Obsolete("This API version has been superseded")]
        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(Industry.BANKING)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatusV1(string industry)
        {
            return Ok(await _statusService.GetSoftwareProductStatusesAsyncV1());
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [CheckXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatus(string industry)
        {
            return Ok(await _statusService.GetSoftwareProductStatusesAsync(industry.ToIndustry()));
        }

        [HttpGet]
        [Route("v1/data-recipients/brands/software-products/status")]
        [CheckXV("1")]
        [ApiVersion("1")]
        [ETag]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatusNoInd()
        {
            return Ok(await _statusService.GetSoftwareProductStatusesAsync());
        }
    }
}