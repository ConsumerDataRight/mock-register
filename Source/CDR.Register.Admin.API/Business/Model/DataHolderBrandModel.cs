using System;
using System.Collections.Generic;
using CDR.Register.Admin.API.Business.Validators;
using CDR.Register.Domain.Entities;
using CDR.Register.Domain.Models;

namespace CDR.Register.Admin.API.Business.Model
{
    public class DataHolderBrandModel
    {
        public Guid DataHolderBrandId { get; set; } = Guid.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string[] Industries { get; set; } = [];

        public string LogoUri { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DataHolderLegalEntityModel? LegalEntity { get; set; } = null;

        public DataHolderEndpointModel? EndpointDetail { get; set; } = null;

        public DataHolderAuthenticationModel? AuthDetails { get; set; } = null;

        public ResponseErrorList Validate(DataHolderBrand existingDataHolderBrand)
        {
            var responseErrorList = new ResponseErrorList();

            // Validate incoming data
            var result = new DataHolderBrandValidator().Validate(this);
            if (!result.IsValid)
            {
                // Validation failed. Handle the error.
                foreach (var error in result.Errors)
                {
                    var detail = error.PropertyName;

                    if (error.ErrorCode.CompareTo("urn:au-cds:error:cds-all:Field/Invalid") == 0)
                    {
                        detail = $"Value '{error.AttemptedValue}' is not allowed for {error.PropertyName}";
                    }

                    responseErrorList.Errors.Add(new Error(error.ErrorCode, error.ErrorMessage, detail));
                }

                return responseErrorList;
            }

            // Validate against the existing data
            var existingDataValidationErrors = this.ValidateWithExisting(existingDataHolderBrand);
            if (existingDataValidationErrors.Count > 0)
            {
                responseErrorList.Errors.AddRange(existingDataValidationErrors);
            }

            return responseErrorList;
        }

        private List<Error> ValidateWithExisting(DataHolderBrand existingDataHolderBrand)
        {
            var errorList = new List<Error>();
            if (existingDataHolderBrand == null)
            {
                return errorList;
            }

            // Validate all the parent IDs.

            // This ensures it is a DH Participation
            if (existingDataHolderBrand.DataHolder == null)
            {
                errorList.Add(new Error(
                    Domain.Constants.ErrorCodes.Cds.InvalidField,
                    Domain.Constants.ErrorTitles.InvalidField,
                    $"Brand {this.DataHolderBrandId} is not a Data Holder."));
            }
            else if (existingDataHolderBrand.DataHolder?.LegalEntity.LegalEntityId != this.LegalEntity?.LegalEntityId)
            {
                errorList.Add(new Error(
                    Domain.Constants.ErrorCodes.Cds.InvalidField,
                    Domain.Constants.ErrorTitles.InvalidField,
                    $"Brand {this.DataHolderBrandId} is already associated with a different legal entity."));
            }
            else if (!string.Equals(existingDataHolderBrand.DataHolder?.Industry, this.Industries[0], StringComparison.InvariantCultureIgnoreCase))
            {
                errorList.Add(new Error(
                    Domain.Constants.ErrorCodes.Cds.InvalidField,
                    Domain.Constants.ErrorTitles.InvalidField,
                    $"Brand {this.DataHolderBrandId} is already associated with the same legal entity in a different industry."));
            }

            return errorList;
        }
    }
}
