using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CDR.Register.Domain.Models
{
    public class ResponseErrorList
    {
        [Required]
        public List<Error> Errors { get; set; }

        public bool HasErrors()
        {
            return Errors != null && Errors.Count > 0;
        }

        public ResponseErrorList()
        {
            Errors = [];
        }

        public ResponseErrorList(Error error)
        {
            Errors = [error];
        }

        public ResponseErrorList(string errorCode, string errorTitle, string errorDetail)
        {
            var error = new Error(errorCode, errorTitle, errorDetail);
            Errors = [error];
        }

        /// <summary>
        /// Add unexpected error to the response error list
        /// </summary>
        public ResponseErrorList AddUnexpectedError(string message)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.UnexpectedError, Constants.ErrorTitles.UnexpectedError, message));
            return this;
        }

        public ResponseErrorList AddUnexpectedError()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.UnexpectedError, Constants.ErrorTitles.UnexpectedError, "An unexpected exception occurred while processing the request."));
            return this;
        }

        /// <summary>
        /// Add invalid industry error to the response error list
        /// </summary>
        public ResponseErrorList AddInvalidIndustry()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "industry"));
            return this;
        }

        // Return Unsupported Version
        public ResponseErrorList AddInvalidXVUnsupportedVersion()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.UnsupportedVersion, Constants.ErrorTitles.UnsupportedVersion, "Requested version is lower than the minimum version or greater than maximum version."));
            return this;
        }

        // Return Invalid Version
        public ResponseErrorList AddInvalidXVInvalidVersion()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidVersion, Constants.ErrorTitles.InvalidVersion, "Version is not a positive Integer."));
            return this;
        }

        public ResponseErrorList AddInvalidXVMissingRequiredHeader()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredHeader, Constants.ErrorTitles.MissingRequiredHeader, "An API version x-v header is required, but was not specified."));
            return this;
        }

        public ResponseErrorList AddInvalidConsentArrangement(string arrangementId)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidConsentArrangement, Constants.ErrorTitles.InvalidConsentArrangement, arrangementId));
            return this;
        }

        public ResponseErrorList AddMissingRequiredHeader(string headerName)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredHeader, Constants.ErrorTitles.MissingRequiredHeader, headerName));
            return this;
        }

        public ResponseErrorList AddMissingRequiredField(string headerName)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.MissingRequiredField, Constants.ErrorTitles.MissingRequiredField, headerName));
            return this;
        }

        public ResponseErrorList AddInvalidField(string fieldName)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, fieldName));
            return this;
        }

        public ResponseErrorList AddInvalidHeader(string headerName)
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidHeader, Constants.ErrorTitles.InvalidHeader, headerName));
            return this;
        }

        public ResponseErrorList AddInvalidDateTime()
        {
            Errors.Add(new Error(Constants.ErrorCodes.Cds.InvalidDateTime, Constants.ErrorTitles.InvalidDateTime, "{0} should be valid DateTimeString"));
            return this;
        }

        public static Error InvalidDateTime()
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidDateTime, Constants.ErrorTitles.InvalidDateTime, "{0} should be valid DateTimeString");
        }

        public static Error InvalidPageSize() //Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidPageSize, Constants.ErrorTitles.InvalidPageSize, "Page size not a positive Integer");
        }

        public static Error PageSizeTooLarge() //Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "Page size too large");
        }

        public static Error InvalidPage() //Should be looked at compared to CDS
        {
            return new Error(Constants.ErrorCodes.Cds.InvalidField, Constants.ErrorTitles.InvalidField, "Page not a positive integer");
        }

        public static Error PageOutOfRange() //Should be looked at compared to CDS
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
    }
}
