using System;

namespace CDR.Register.SSA.API.Business.Models
{
    public class SsaValidationException : Exception
    {
        public SsaValidationException()
        {
        }

        public SsaValidationException(string message)
            : base(message)
        {
        }
    }
}
