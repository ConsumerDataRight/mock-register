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
                Code = "Field/InvalidIndustry",
                Title = "Invalid Industry",
                Detail = "",
                Meta = new object()
            });
            return this;
        }

        /// <summary>
        /// Add invalid x-v error to the response error list
        /// </summary>
        /// <param name="meta"></param>
        public ResponseErrorList InvalidXV()
        {
            Errors.Add(new Error()
            {
                Code = "Header/UnsupportedVersion",
                Title = "Unsupported Version",
                Detail = "",
                Meta = new object()
            });

            return this;
        }

        public static Error InvalidDateTime()
        {
            return new Error("Field/InvalidDateTime", "Invalid DateTime", "{0} should be valid DateTimeString");
        }

        public static Error InvalidPageSize()
        {
            return new Error("Field/InvalidPageSize", "Invalid Page Size", "Page size not a positive Integer");
        }

        public static Error PageSizeTooLarge()
        {
            return new Error("Field/InvalidPageSizeTooLarge", "Page Size Too Large", string.Empty);
        }

        public static Error InvalidPage()
        {
            return new Error("Field/InvalidPage", "Invalid Page", "Page not a positive Integer");
        }

        public static Error PageOutOfRange()
        {
            return new Error("Field/InvalidPageOutOfRange", "Page Is Out Of Range", string.Empty);
        }

        public static Error DataRecipientParticipationNotActive()
        {
            return new Error("Authorisation/AdrStatusNotActive", "ADR Status Is Not Active", string.Empty);
        }

        public static Error DataRecipientSoftwareProductNotActive()
        {
            return new Error("Authorisation/AdrSoftwareProductStatusNotActive", "ADR Software Product Status Is Not Active", string.Empty);
        }

        public static Error InvalidSoftwareProduct()
        {
            return new Error("Field/InvalidSoftwareProduct", "Invalid Software Product", string.Empty);
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
