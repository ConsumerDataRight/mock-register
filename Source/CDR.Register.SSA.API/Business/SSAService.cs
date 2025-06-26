using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CDR.Register.SSA.API.Business
{
    public class SsaService : ISsaService
    {
        private readonly ILogger<SsaService> _logger;
        private readonly IMapper _mapper;
        private readonly ISoftwareStatementAssertionRepository _repository;
        private readonly ITokenizerService _tokenizer;

        public SsaService(
            ILogger<SsaService> logger,
            IConfiguration configuration,
            IMapper mapper,
            ISoftwareStatementAssertionRepository repository,
            ITokenizerService tokenizer)
        {
            this._logger = logger;
            this._mapper = mapper;
            this._repository = repository;
            this._tokenizer = tokenizer;
        }

        public async Task<string> GetSoftwareStatementAssertionJWTAsync(Repository.Infrastructure.Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await this.GetSoftwareStatementAssertionAsync(industry, dataRecipientBrandId, softwareProductId);
            return await this._tokenizer.GenerateJwtTokenAsync(ssa);
        }

        public async Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsync(Repository.Infrastructure.Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            var softwareProductEntity = await this.GetSoftwareStatementAssertionAsync(dataRecipientBrandId, softwareProductId);
            var ssa = this._mapper.MapV3(softwareProductEntity);
            if (ssa == null)
            {
                return null;
            }

            using (LogContext.PushProperty("MethodName", "GetSoftwareStatementAssertionAsync"))
            {
                this._logger.LogDebug("SSA for dataRecipientBrandId: {DataRecipientBrandId} / softwareProductId: {SoftwareProductId} \r\n{Ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());
            }

            // Validate the SSA
            Validate(ssa);

            return ssa;
        }

        private static void Validate(SoftwareStatementAssertionModel ssa)
        {
            var validationContext = new ValidationContext(ssa);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(ssa, validationContext, validationResults, true))
            {
                var errorMessage = $"Validation errors in SSA for dataRecipientBrandId: {ssa.Org_id} / softwareProductId: {ssa.Software_id} \r\n{validationResults.ToJson()}";
                throw new SsaValidationException(errorMessage);
            }
        }

        private async Task<SoftwareStatementAssertion> GetSoftwareStatementAssertionAsync(string dataRecipientBrandId, string softwareProductId)
        {
            if (string.IsNullOrWhiteSpace(dataRecipientBrandId) || string.IsNullOrWhiteSpace(softwareProductId))
            {
                return null;
            }

            var dataRecipientBrandGuid = Guid.Parse(dataRecipientBrandId);
            var softwareProductGuid = Guid.Parse(softwareProductId);
            return await this._repository.GetSoftwareStatementAssertionAsync(dataRecipientBrandGuid, softwareProductGuid);
        }
    }
}
