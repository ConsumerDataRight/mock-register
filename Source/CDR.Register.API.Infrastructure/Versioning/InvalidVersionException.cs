using System;
using System.Runtime.Serialization;

namespace CDR.Register.API.Infrastructure.Versioning
{
    [Serializable]
    public class InvalidVersionException : Exception
    {
        public string HeaderName { get; set; }

        public InvalidVersionException() : base() { }

        public InvalidVersionException(string headerName) : base() 
        {
            this.HeaderName = headerName;
        }

        protected InvalidVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
