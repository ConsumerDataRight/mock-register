using CDR.Register.API.Infrastructure;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.SSA.API.Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CDR.Register.SSA.API.Business
{
    public class SSAService : ISSAService
    {
        private readonly ILogger<SSAService> _logger;
        private readonly IMapper _mapper;
        private readonly ISoftwareStatementAssertionRepository _repository;
        private readonly ITokenizerService _tokenizer;

        public SSAService(
            ILogger<SSAService> logger,
            IConfiguration configuration,
            IMapper mapper,
            ISoftwareStatementAssertionRepository repository,
            ITokenizerService tokenizer)
        {
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _tokenizer = tokenizer;
        }

        public async Task<string> GetSoftwareStatementAssertionJWTAsyncXV1(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await GetSoftwareStatementAssertionAsyncXV1(industry, dataRecipientBrandId, softwareProductId);
            FilterBankingScopes(ssa);
            return await _tokenizer.GenerateJwtTokenAsync(ssa);
        }

        public async Task<SoftwareStatementAssertionModel> GetSoftwareStatementAssertionAsyncXV1(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            var softwareProductEntity = await GetSoftwareStatementAssertionAsync(dataRecipientBrandId, softwareProductId);
            var ssa = _mapper.Map(softwareProductEntity);
            if (ssa == null)
            {
                return null;
            }

            using (LogContext.PushProperty("MethodName", "GetSoftwareStatementAssertionAsync"))
            {
                _logger.LogDebug("SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());
            }

            // Validate the SSA
            Validate(ssa);

            return ssa;
        }

        public async Task<string> GetSoftwareStatementAssertionJWTAsyncXV2(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await GetSoftwareStatementAssertionAsyncXV2(industry, dataRecipientBrandId, softwareProductId);
            FilterBankingScopes(ssa);
            return await _tokenizer.GenerateJwtTokenAsync(ssa);
        }

        public async Task<SoftwareStatementAssertionModelV2> GetSoftwareStatementAssertionAsyncXV2(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            var softwareProductEntity = await GetSoftwareStatementAssertionAsync(dataRecipientBrandId, softwareProductId);
            var ssa = _mapper.MapV2(softwareProductEntity);
            if (ssa == null)
            {
                return null;
            }

            using (LogContext.PushProperty("MethodName", "GetSoftwareStatementAssertionAsync"))
            {
                _logger.LogDebug("SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());
            }

            // Validate the SSA
            Validate(ssa);

            return ssa;
        }

        public async Task<string> GetSoftwareStatementAssertionJWTAsyncXV3(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            // Get the SSA to be put in a JWT
            var ssa = await GetSoftwareStatementAssertionAsyncXV3(industry, dataRecipientBrandId, softwareProductId);
            return await _tokenizer.GenerateJwtTokenAsync(ssa);
        }

        public async Task<SoftwareStatementAssertionModelV3> GetSoftwareStatementAssertionAsyncXV3(Industry industry, string dataRecipientBrandId, string softwareProductId)
        {
            var softwareProductEntity = await GetSoftwareStatementAssertionAsync(dataRecipientBrandId, softwareProductId);
            var ssa = _mapper.MapV3(softwareProductEntity);
            if (ssa == null)
            {
                return null;
            }

            using (LogContext.PushProperty("MethodName", "GetSoftwareStatementAssertionAsync"))
            {
                _logger.LogDebug("SSA for dataRecipientBrandId: {dataRecipientBrandId} / softwareProductId: {softwareProductId} \r\n{ssa}", dataRecipientBrandId, softwareProductId, ssa.ToJson());
            }

            // Validate the SSA
            Validate(ssa);

            return ssa;
        }

        private void Validate(SoftwareStatementAssertionModel ssa)
        {
            var validationContext = new ValidationContext(ssa);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(ssa, validationContext, validationResults, true))
            {
                var errorMessage = $"Validation errors in SSA for dataRecipientBrandId: {ssa.org_id} / softwareProductId: {ssa.software_id} \r\n{validationResults.ToJson()}";
                throw new SSAValidationException(errorMessage);
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
            return await _repository.GetSoftwareStatementAssertionAsync(dataRecipientBrandGuid, softwareProductGuid);
        }

        /// <summary>
        /// Used by the v1 and v2 Get SSA API to only return banking related scopes.
        /// </summary>
        private static void FilterBankingScopes(SoftwareStatementAssertionModel ssa)
        {
            if (ssa == null || string.IsNullOrEmpty(ssa.scope))
            {
                return;
            }

            var scopes = ssa.scope.Split(' ');
            var allowedScopes = new string[]
            {
                "openid",
                "profile",
                "bank:accounts.basic:read",
                "bank:accounts.detail:read",
                "bank:transactions:read",
                "bank:payees:read",
                "bank:regular_payments:read",
                "common:customer.basic:read",
                "common:customer.detail:read",
                "cdr:registration",
                "introspect_tokens",
                "revoke_tokens",
            };

            // Filter the provided list of scopes with the allowed scopes.
            ssa.scope = String.Join(' ', scopes.Where(s => allowedScopes.Contains(s)));
        }

    }
}