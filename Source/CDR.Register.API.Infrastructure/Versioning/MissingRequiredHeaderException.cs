using System;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class MissingRequiredHeaderException : Exception
    {
        public MissingRequiredHeaderException()
            : base()
        {
            this.HeaderName = string.Empty;
        }

        public MissingRequiredHeaderException(string headerName)
            : base()
        {
            this.HeaderName = headerName;
        }

        public string HeaderName { get; set; }
    }
}
