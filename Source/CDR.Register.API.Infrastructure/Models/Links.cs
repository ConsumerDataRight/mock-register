using System;

namespace CDR.Register.API.Infrastructure.Models
{
    public class Links
    {
        /// <summary>
        /// Gets or sets fully qualified link to this API call.
        /// </summary>
        public Uri? Self { get; set; }
    }
}
