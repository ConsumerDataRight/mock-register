using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CDR.Register.Domain.Repositories;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.Extensions.Logging;

namespace CDR.Register.SSA.API.Business
{
    public class SSAService : ISSAService
    {
        private readonly ILogger<SSAService> _logger;
        private readonly IMapper _mapper;
        private readonly ISoftwareStatementAssertionRepository _repository;
        private readonly ITokenizerService _tokenizer;

        public SSAService(ILogger<SSAService> logger, IMapper mapper, ISoftwareStatementAssertionRepository repository, ITokenizerService tokenizer)
        {
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _tokenizer = tokenizer;
        }

        public async Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsync(
            string dataRecipientBrandId, 
            string softwareProductId)
        {
            if (string.IsNullOrWhiteSpace(dataRecipientBrandId) || string.IsNullOrWhiteSpace(softwareProductId))
            {
                return null;
            }

            var dataRecipientBrandGuid = Guid.Parse(dataRecipientBrandId);
            var softwareProductGuid = Guid.Parse(softwareProductId);

            var softwareProductEntity = await _repository.GetSoftwareStatementAssertionAsync(dataRecipientBrandGuid, softwareProductGuid);
            var ssa = _mapper.Map(softwareProductEntity);
            if (ssa == null)
            {
                return null;
            }

            _logger.LogDebug("SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());

            // Validate the SSA
            var validationContext = new ValidationContext(ssa);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(ssa, validationContext, validationResults, true))
            {
                var errorMessage = $"Validation errors in SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{validationResults.ToJson()}";
                throw new SSAValidationException(errorMessage);
            }

            return ssa;
        }

        public async Task<Models.SoftwareStatementAssertionV2Model> GetSoftwareStatementAssertionV2Async(
            string dataRecipientBrandId, 
            string softwareProductId)
        {
            if (string.IsNullOrWhiteSpace(dataRecipientBrandId) || string.IsNullOrWhiteSpace(softwareProductId))
            {
                return null;
            }

            var dataRecipientBrandGuid = Guid.Parse(dataRecipientBrandId);
            var softwareProductGuid = Guid.Parse(softwareProductId);

            var softwareProduct = await _repository.GetSoftwareStatementAssertionAsync(dataRecipientBrandGuid, softwareProductGuid);
            if (softwareProduct == null)
            {
                return null;
            }

            var ssa = _mapper.MapV2(softwareProduct);

            _logger.LogDebug("SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());

            // Validate the SSA
            var validationContext = new ValidationContext(ssa);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(ssa, validationContext, validationResults, true))
            {
                var errorMessage = $"Validation errors in SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{validationResults.ToJson()}";
                throw new SSAValidationException(errorMessage);
            }

            return ssa;
        }

        public async Task<string> GetSoftwareStatementAssertionJWTAsync(string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await GetSoftwareStatementAssertionAsync(dataRecipientBrandId, softwareProductId);
            return await _tokenizer.GenerateJwtTokenAsync(ssa);
        }

        public async Task<string> GetSoftwareStatementAssertionJWTV2Async(string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await GetSoftwareStatementAssertionV2Async(dataRecipientBrandId, softwareProductId);
            return await _tokenizer.GenerateJwtTokenAsync(ssa);
        }
    }
}
