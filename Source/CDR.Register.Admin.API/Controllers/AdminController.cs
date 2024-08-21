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
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
            _logger = logger;
            _dbContext = dbContext;
            _adminRepository = registerAdminRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("Metadata")]
        [ApiVersionNeutral]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task LoadData()
        {
            using var reader = new StreamReader(Request.Body);
            string json = await reader.ReadToEndAsync();
            string respMsg = "";

            try
            {
                bool updated = await _dbContext.SeedDatabaseFromJson(json, _logger, true);
                if (updated)
                    Response.StatusCode = StatusCodes.Status200OK;
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    respMsg = "Database not updated";
                }
            }
            catch
            {
                // SeedDatabaseFromJson doesn't throw specific error exceptions, so lets just consider any exception a BadRequest
                Response.StatusCode = StatusCodes.Status400BadRequest;
                respMsg = "UnexpectedError, An error occurred loading the database.";
            }
            finally
            {
                Response.ContentType = "application/json";
                await Response.BodyWriter.WriteAsync(System.Text.Encoding.UTF8.GetBytes(respMsg));
            }
        }

        [HttpGet]
        [Route("Metadata")]
        [ApiVersionNeutral]
        [ServiceFilter(typeof(LogActionEntryAttribute))]
        public async Task GetData()
        {
            //We need to override the usual default settings so that the FK Ids can use their int values instead of their string values
            var apiSpecificSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            JsonConvert.DefaultSettings = () => apiSpecificSettings;

            var metadata = await _dbContext.GetJsonFromDatabase();

            // Return the raw JSON response.
            Response.ContentType = "application/json";
            await Response.BodyWriter.WriteAsync(System.Text.UTF8Encoding.UTF8.GetBytes($"{{ \"legalEntities\": {metadata} }}"));
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
                var existingDataHolderBrand = await _adminRepository.GetDataHolderBrandAsync(dataHolderBrandModel.DataHolderBrandId);

                // Validate the incoming data holder model.
                var validationErrors = dataHolderBrandModel.Validate(existingDataHolderBrand);
                if (validationErrors != null && validationErrors.Errors.Count > 0)
                {
                    return BadRequest(validationErrors);
                }

                var dataHolderBrandToSave = _mapper.Map<DataHolderBrand>(dataHolderBrandModel);
                var isDhBrandSaved = await _adminRepository.SaveDataHolderBrand(dataHolderBrandToSave.DataHolder.LegalEntity.LegalEntityId, dataHolderBrandToSave);
                if (!isDhBrandSaved)
                {
                    // Return error message here
                    return BadRequest();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to the save data holder.");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                using var reader = new StreamReader(Request.Body);
                string json = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(json))
                {
                    return BadRequest(new Error(Domain.Constants.ErrorTitles.InvalidField, Domain.Constants.ErrorCodes.Cds.InvalidField, "Empty LegalEntity received"));
                }

                var legalEntity = System.Text.Json.JsonSerializer.Deserialize<LegalEntity>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); //This is inconsistent with other serializers

                var errors = legalEntity?.GetValidationErrors(new LegalEntityValidator());
                if (errors?.Errors.Count > 0)
                {
                    return BadRequest(errors);
                }

                var dataRecipient = _mapper.Map<DataRecipient>(legalEntity);
                var businessRuleError = await _adminRepository.AddOrUpdateDataRecipient(dataRecipient);
                return businessRuleError switch
                {
                    null => Ok(),
                    _ => BadRequest(businessRuleError.ToResponseErrorList())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured in SaveDataRecipient");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
