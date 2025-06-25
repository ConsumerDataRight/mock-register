using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Status.API.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace CDR.Register.Status.API.Controllers
{
    [Route("cdr-register")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;
        private readonly IConfiguration _configuration;

        public StatusController(
            IStatusService statusService,
            IConfiguration configuration)
        {
            this._statusService = statusService;
            this._configuration = configuration;
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [ReturnXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetDataRecipientsStatusXV2(string industry)
        {
            var response = await this._statusService.GetDataRecipientStatusesAsync(industry.ToIndustry());
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [ReturnXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetSoftwareProductStatusXV2(string industry)
        {
            var response = await this._statusService.GetSoftwareProductStatusesAsync(industry.ToIndustry());
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }

        [HttpGet]
        [Route("v1/{industry}/data-holders/status")]
        [ReturnXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task<IStatusCodeActionResult> GetDataHolderStatusXV1(string industry)
        {
            var response = await this._statusService.GetDataHolderStatusesAsyncXV1(industry.ToIndustry());
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }
    }
}
