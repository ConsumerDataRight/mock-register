using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDR.Register.API.Infrastructure.Versioning
{
    [Serializable]
    public class MissingRequiredHeaderException : Exception
    {
        public string HeaderName { get; set; }

        public MissingRequiredHeaderException() : base() { }

        public MissingRequiredHeaderException(string headerName) : base()
        {
            this.HeaderName = headerName;
        }

        protected MissingRequiredHeaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
