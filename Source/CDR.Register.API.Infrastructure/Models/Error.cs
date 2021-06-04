using System.ComponentModel.DataAnnotations;

namespace CDR.Register.API.Infrastructure.Models
{
    public class Error
    {
        public Error()
        {
            this.Meta = new object();
        }

        public Error(string code, string title, string detail) : this()
        {
            this.Code = code;
            this.Title = title;
            this.Detail = detail;
        }

        /// <summary>
        /// Error code
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Error title
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Error detail
        /// </summary>
        [Required]
        public string Detail { get; set; }

        /// <summary>
        /// Optional additional data for specific error types
        /// </summary>
        public object Meta { get; set; }
    }
}
