using System;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class MissingRequiredHeaderException : Exception
    {
        public string HeaderName { get; set; }

        public MissingRequiredHeaderException() : base() 
        {
            HeaderName = string.Empty;
        }

        public MissingRequiredHeaderException(string headerName) : base()
        {
            this.HeaderName = headerName;
        }        
    }
}
