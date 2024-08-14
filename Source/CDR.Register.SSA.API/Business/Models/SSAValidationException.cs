using System;
using System.Runtime.Serialization;

namespace CDR.Register.SSA.API.Business.Models
{
    
    public class SSAValidationException : Exception
    {
        public SSAValidationException()
        {

        }

        public SSAValidationException(string message) : base(message)
        {

        }
    }

}
