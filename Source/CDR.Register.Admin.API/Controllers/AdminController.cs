using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Admin.API.Business.Validators;
using CDR.Register.Admin.API.Extensions;
using CDR.Register.Admin.API.Filters;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Models;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CDR.Register.Admin.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly RegisterDatabaseContext _dbContext;
        private readonly IRegisterAdminRepository _adminRepository;
        private readonly IMapper _mapper;

        public AdminController(ILogger<AdminController> logger, RegisterDatabaseContext dbContext, IRegisterAdminRepository registerAdminRepository, IMapper mapper)
        {
            this._logger = logger;
            this._dbContext = dbContext;
            this._adminRepository = registerAdminRepository;
            this._mapper = mapper;
        }

        [HttpPost]
        [Route("Metadata")]
        [ApiVersionNeutral]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task LoadData()
        {
            using var reader = new StreamReader(this.Request.Body);
            string json = await reader.ReadToEndAsync();
            string respMsg = string.Empty;

            try
            {
                bool updated = await this._dbContext.SeedDatabaseFromJson(json, this._logger, true);
                if (updated)
                {
                    this.Response.StatusCode = StatusCodes.Status200OK;
                }
                else
                {
                    this.Response.StatusCode = StatusCodes.Status400BadRequest;
                    respMsg = "Database not updated";
                }
            }
            catch
            {
                // SeedDatabaseFromJson doesn't throw specific error exceptions, so lets just consider any exception a BadRequest
                this.Response.StatusCode = StatusCodes.Status400BadRequest;
                respMsg = "UnexpectedError, An error occurred loading the database.";
            }
            finally
            {
                this.Response.ContentType = "application/json";
                await this.Response.BodyWriter.WriteAsync(System.Text.Encoding.UTF8.GetBytes(respMsg));
            }
        }

        [HttpGet]
        [Route("Metadata")]
        [ApiVersionNeutral]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task GetData()
        {
            // We need to override the usual default settings so that the FK Ids can use their int values instead of their string values
            var apiSpecificSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            JsonConvert.DefaultSettings = () => apiSpecificSettings;

            var metadata = await this._dbContext.GetJsonFromDatabase();

            // Return the raw JSON response.
            this.Response.ContentType = "application/json";
            await this.Response.BodyWriter.WriteAsync(System.Text.UTF8Encoding.UTF8.GetBytes($"{{ \"legalEntities\": {metadata} }}"));
        }

        [HttpPost]
        [Route("metadata/data-holders")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        [ApiVersion("1")]
        [ReturnXV("1")]
        [ApiAuthorize]
        public async Task<IActionResult> SaveDataHolderBrand([FromBody] DataHolderBrandModel dataHolderBrandModel)
        {
            try
            {
                // Get the existing data holder record
                var existingDataHolderBrand = await this._adminRepository.GetDataHolderBrandAsync(dataHolderBrandModel.DataHolderBrandId);

                // Validate the incoming data holder model.
                var validationErrors = dataHolderBrandModel.Validate(existingDataHolderBrand);
                if (validationErrors != null && validationErrors.Errors.Count > 0)
                {
                    return this.BadRequest(validationErrors);
                }

                var dataHolderBrandToSave = this._mapper.Map<DataHolderBrand>(dataHolderBrandModel);
                var isDhBrandSaved = await this._adminRepository.SaveDataHolderBrand(dataHolderBrandToSave.DataHolder.LegalEntity.LegalEntityId, dataHolderBrandToSave);
                if (!isDhBrandSaved)
                {
                    // Return error message here
                    return this.BadRequest();
                }

                return this.Ok();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "An error occurred while trying to the save data holder.");
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("metadata/data-recipients")]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        [ApiVersion("1")]
        [ReturnXV("1")]
        [ApiAuthorize]
        public async Task<ActionResult> SaveDataRecipient()
        {
            try
            {
                using var reader = new StreamReader(this.Request.Body);
                string json = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    return this.BadRequest(new Error(Domain.Constants.ErrorTitles.InvalidField, Domain.Constants.ErrorCodes.Cds.InvalidField, "Empty LegalEntity received"));
                }

                var legalEntity = System.Text.Json.JsonSerializer.Deserialize<LegalEntity>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // This is inconsistent with other serializers

                var errors = legalEntity?.GetValidationErrors(new LegalEntityValidator());
                if (errors?.Errors.Count > 0)
                {
                    return this.BadRequest(errors);
                }

                var dataRecipient = this._mapper.Map<DataRecipient>(legalEntity);
                var businessRuleError = await this._adminRepository.AddOrUpdateDataRecipient(dataRecipient);
                return businessRuleError switch
                {
                    null => this.Ok(),
                    _ => this.BadRequest(businessRuleError.ToResponseErrorList()),
                };
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "An error occured in SaveDataRecipient");
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
