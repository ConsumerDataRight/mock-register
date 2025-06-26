using System;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class InvalidVersionException : Exception
    {
        public InvalidVersionException()
            : base()
        {
        }

        public InvalidVersionException(string headerName)
            : base()
        {
            this.HeaderName = headerName;
        }

        public string HeaderName { get; set; } = string.Empty;
    }
}
