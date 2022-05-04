using System;
using System.Runtime.Serialization;

namespace CDR.Register.API.Infrastructure.Versioning
{
    [Serializable]
    public class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException() : base() { }

        public UnsupportedVersionException(string message) : base(message) { }

        public UnsupportedVersionException(int minVersion, int maxVersion)
            : base($"minimum version: {minVersion}, maximum version: {maxVersion}")
        {
        }

        protected UnsupportedVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
