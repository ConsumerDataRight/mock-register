using System.ComponentModel.DataAnnotations;
using CDR.Register.Domain.Extensions;

namespace CDR.Register.Domain.Models
{
    public class Error
    {
        public Error()
        {
        }

        public Error(string code, string title, string detail)
        {
            this.Code = code;
            this.Title = title;
            this.Detail = detail;
        }

        public Error(string code, string title, string detail, string metaUrn)
        {
            this.Code = code;
            this.Title = title;
            this.Detail = detail;
            this.Meta = metaUrn.IsNullOrWhiteSpace() ? null : new MetaError(metaUrn);
        }

        /// <summary>
        /// Gets or sets Error code.
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets Error title.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Error detail.
        /// </summary>
        [Required]
        public string Detail { get; set; }

        /// <summary>
        /// Gets or sets Optional additional data for specific error types.
        /// </summary>
        public MetaError Meta { get; set; }
    }
}
