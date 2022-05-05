using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CDR.Register.API.Infrastructure.Models
{
    public class ResponseErrorList
    {
        [Required]
        public List<Error> Errors { get; set; }

        public bool HasErrors()
        {
            return Errors != null && Errors.Any();
        }

        public ResponseErrorList()
        {
            this.Errors = new List<Error>();
        }

        public ResponseErrorList(Error error)
        {
            this.Errors = new List<Error>() { error };
        }

        public ResponseErrorList(string errorCode, string errorTitle, string errorDetail)
        {
            var error = new Error(errorCode, errorTitle, errorDetail);
            this.Errors = new List<Error>() { error };
        }

        /// <summary>
        /// Add invalid industry error to the response error list
        /// </summary>
        /// <param name="meta"></param>
        public ResponseErrorList InvalidIndustry()
        {
            Errors.Add(new Error()
            {
                Code = "urn:au-cds:error:cds-all:Field/Invalid",
                Title = "Invalid Field",
                Detail = "industry",
                Meta = new object()
            });
            return this;
        }

        // Return Unsupported Version
        public ResponseErrorList InvalidXVUnsupportedVersion(string detail = null)
        {
            Errors.Add(new Error()
            {
                Code = "urn:au-cds:error:cds-all:Header/UnsupportedVersion",
                Title = "Unsupported Version",
                Detail = detail,
                Meta = new object()
            });

            return this;
        }

        // Return Invalid Version
        public ResponseErrorList InvalidXVInvalidVersion()
        {
            Errors.Add(new Error()
            {
                Code = "urn:au-cds:error:cds-all:Header/InvalidVersion",
                Title = "Invalid Version",
                Detail = "",
                Meta = new object()
            });

            return this;
        }

        // Return Missing Required Header
        public ResponseErrorList InvalidXVMissingRequiredHeader()
        {
            Errors.Add(new Error()
            {
                Code = "urn:au-cds:error:cds-all:Header/Missing",
                Title = "Missing Required Header",
                Detail = "",
                Meta = new object()
            });

            return this;
        }

        public static Error InvalidDateTime()
        {
            return new Error("urn:au-cds:error:cds-all:Field/InvalidDateTime", "Invalid DateTime", "{0} should be valid DateTimeString");
        }

        public static Error InvalidPageSize()
        {
            return new Error("urn:au-cds:error:cds-all:Field/InvalidPageSize", "Invalid Page Size", "Page size not a positive Integer");
        }

        public static Error PageSizeTooLarge()
        {
            return new Error("urn:au-cds:error:cds-all:Field/Invalid", "Invalid Field", "Page size too large");
        }

        public static Error InvalidPage()
        {
            return new Error("urn:au-cds:error:cds-all:Field/Invalid", "Invalid Field", "Page not a positive integer");
        }

        public static Error PageOutOfRange()
        {
            return new Error("urn:au-cds:error:cds-all:Field/Invalid", "Invalid Field", "Page is out of range");
        }

        public static Error DataRecipientParticipationNotActive()
        {
            return new Error("urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive", "ADR Status Is Not Active", string.Empty);
        }

        public static Error DataRecipientSoftwareProductNotActive()
        {
            return new Error("urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive", "ADR Status Is Not Active", string.Empty);
        }

        public static Error InvalidResource(string softwareProductId)
        {
            return new Error("urn:au-cds:error:cds-all:Resource/Invalid", "Invalid Resource", softwareProductId);
        }

        public static Error InvalidSoftwareProduct(string softwareProductId)
        {
            return new Error("urn:au-cds:error:cds-register:Field/InvalidSoftwareProduct", "Invalid Software Product", softwareProductId);
        }

        public static Error InvalidBrand()
        {
            return new Error("Field/InvalidBrand", "Invalid Brand", string.Empty);
        }

        public static Error NotFound()
        {
            return new Error("Resource/NotFound", "Resource Not Found", string.Empty);
        }

        public static Error UnknownError()
        {
            return new Error("Unknown", "Unknown error", string.Empty);
        }
    }
}
