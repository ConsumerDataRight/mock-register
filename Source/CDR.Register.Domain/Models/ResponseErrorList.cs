using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Domain.Models
{
    public class ResponseErrorList
    {
        public ResponseErrorList()
        {
            this.Errors = [];
        }

        public ResponseErrorList(Error error)
        {
            this.Errors = [error];
        }

        public ResponseErrorList(string errorCode, string errorTitle, string errorDetail)
        {
            var error = new Error(errorCode, errorTitle, errorDetail);
            this.Errors = [error];
        }

        [Required]
        public List<Error> Errors { get; set; }

        public static Error InvalidDateTime()
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidDateTime, Constants.ErrorTitles.InvalidDateTime, "{0} should be valid DateTimeString");
        }

        public static Error InvalidPageSize() // Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidPageSize, Constants.ErrorTitles.InvalidPageSize, "Page size not a positive Integer");
        }

        public static Error PageSizeTooLarge() // Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "Page size too large");
        }

        public static Error InvalidPage() // Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "Page not a positive integer");
        }

        public static Error PageOutOfRange() // Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "Page is out of range");
        }

        public static Error DataRecipientParticipationNotActive()
        {
            return new Error(Constants.ErrorCodes.Cds.AdrStatusNotActive, Constants.ErrorTitles.ADRStatusNotActive, string.Empty);
        }

        public static Error DataRecipientSoftwareProductNotActive()
        {
            return new Error(Constants.ErrorCodes.Cds.AdrStatusNotActive, Constants.ErrorTitles.ADRStatusNotActive, string.Empty);
        }

        public static Error InvalidResource(string softwareProductId)
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidResource, Constants.ErrorTitles.InvalidResource, softwareProductId);
        }

        public static Error InvalidSoftwareProduct(string softwareProductId)
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidSoftwareProduct, Constants.ErrorTitles.InvalidSoftwareProduct, softwareProductId);
        }

        public static Error NotFound()
        {
            return new Error(Constants.ErrorCodes.Cds.ResourceNotFound, Constants.ErrorTitles.ResourceNotFound, string.Empty);
        }

        public bool HasErrors()
        {
            return this.Errors != null && this.Errors.Count > 0;
        }

        /// <summary>
        /// Add unexpected error to the response error list.
        /// </summary>
        /// <returns>ErrorList for response.</returns>
        public ResponseErrorList AddUnexpectedError(string message)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.UnexpectedError, Constants.ErrorTitles.UnexpectedError, message));
            return this;
        }

        public ResponseErrorList AddUnexpectedError()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.UnexpectedError, Constants.ErrorTitles.UnexpectedError, "An unexpected exception occurred while processing the request."));
            return this;
        }

        /// <summary>
        /// Add invalid industry error to the response error list.
        /// </summary>
        /// <returns>Errorlist for response.</returns>
        public ResponseErrorList AddInvalidIndustry()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "industry"));
            return this;
        }

        // Return Unsupported Version
        public ResponseErrorList AddInvalidXVUnsupportedVersion()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.UnsupportedVersion, Constants.ErrorTitles.UnsupportedVersion, "Requested version is lower than the minimum version or greater than maximum version."));
            return this;
        }

        // Return Invalid Version
        public ResponseErrorList AddInvalidXVInvalidVersion()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidVersion, Constants.ErrorTitles.InvalidVersion, "Version is not a positive Integer."));
            return this;
        }

        public ResponseErrorList AddInvalidXVMissingRequiredHeader()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredHeader, Constants.ErrorTitles.MissingRequiredHeader, "An API version x-v header is required, but was not specified."));
            return this;
        }

        public ResponseErrorList AddInvalidConsentArrangement(string arrangementId)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidConsentArrangement, Constants.ErrorTitles.InvalidConsentArrangement, arrangementId));
            return this;
        }

        public ResponseErrorList AddMissingRequiredHeader(string headerName)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredHeader, Constants.ErrorTitles.MissingRequiredHeader, headerName));
            return this;
        }

        public ResponseErrorList AddMissingRequiredField(string headerName)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredField, Constants.ErrorTitles.MissingRequiredField, headerName));
            return this;
        }

        public ResponseErrorList AddInvalidField(string fieldName)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, fieldName));
            return this;
        }

        public ResponseErrorList AddInvalidHeader(string headerName)
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidHeader, Constants.ErrorTitles.InvalidHeader, headerName));
            return this;
        }

        public ResponseErrorList AddInvalidDateTime()
        {
            this.Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidDateTime, Constants.ErrorTitles.InvalidDateTime, "{0} should be valid DateTimeString"));
            return this;
        }
    }
}
