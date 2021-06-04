using System;
using System.Runtime.Serialization;

namespace CDR.Register.SSA.API.Business.Models
{
    [Serializable]
    public class SSAValidationException : Exception
    {
        protected SSAValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }

        public SSAValidationException()
        {

        }

        public SSAValidationException(string message) : base(message)
        {

        }
    }

}
