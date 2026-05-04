using System.Threading.Tasks;
using Asp.Versioning;
using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Repository.Infrastructure;
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
        [CheckIndustry(Industry.BANKING, Industry.ENERGY, Industry.TELCO, Industry.ALL)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetDataRecipientsStatusXV2(string industry)
            => this.GetDataRecipientsStatus(industry, 2);

        [HttpGet]
        [Route("v1/{industry}/data-recipients/status")]
        [ReturnXV("3")]
        [ApiVersion("3")]
        [ETag]
        [CheckIndustry(Industry.ALL)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetDataRecipientsStatusXV3(string industry)
            => this.GetDataRecipientsStatus(industry, 3);

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [ReturnXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry(Industry.ALL, Industry.BANKING, Industry.ENERGY, Industry.TELCO)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetSoftwareProductStatusXV2(string industry)
            => this.GetSoftwareProductStatus(industry, 2);

        [HttpGet]
        [Route("v1/{industry}/data-recipients/brands/software-products/status")]
        [ReturnXV("3")]
        [ApiVersion("3")]
        [ETag]
        [CheckIndustry(Industry.ALL)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetSoftwareProductStatusXV3(string industry)
            => this.GetSoftwareProductStatus(industry, 3);

        [HttpGet]
        [Route("v1/{industry}/data-holders/status")]
        [ReturnXV("1")]
        [ApiVersion("1")]
        [ETag]
        [CheckIndustry(Industry.ALL, Industry.BANKING, Industry.ENERGY, Industry.TELCO)]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetDataHolderStatusXV1(string industry)
            => this.GetDataHolderStatus(industry, 1);

        [HttpGet]
        [Route("v1/{industry}/data-holders/status")]
        [ReturnXV("2")]
        [ApiVersion("2")]
        [ETag]
        [CheckIndustry]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public Task<IStatusCodeActionResult> GetDataHolderStatusXV2(string industry)
            => this.GetDataHolderStatus(industry, 2);

        private async Task<IStatusCodeActionResult> GetDataHolderStatus(string industry, int version)
        {
            var response = await this._statusService.GetDataHolderStatuses(industry.ToIndustry(), version);
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }

        private async Task<IStatusCodeActionResult> GetDataRecipientsStatus(string industry, int version)
        {
            var response = await this._statusService.GetDataRecipientStatuses(industry.ToIndustry(), version);
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }

        private async Task<IStatusCodeActionResult> GetSoftwareProductStatus(string industry, int version)
        {
            var response = await this._statusService.GetSoftwareProductStatuses(industry.ToIndustry(), version);
            response.Links = this.GetSelf(this._configuration, this.HttpContext, string.Empty);
            return this.Ok(response);
        }
    }
}
