using System;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class UnsupportedVersionException : Exception
    {
        public UnsupportedVersionException() : base() { }

        public UnsupportedVersionException(string message) : base(message) { }

        public UnsupportedVersionException(int minVersion, int maxVersion)
            : base($"minimum version: {minVersion}, maximum version: {maxVersion}")
        {
        }        
    }
}
