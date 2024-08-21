namespace CDR.Register.API.Infrastructure.Models
{
    public class CdrApiEndpointVersionOptions
    {
        public readonly string Path;
        public readonly bool IsVersioned;
        public readonly bool IsXVHeaderMandatory;
        public readonly int? CurrentMinVersion;
        public readonly int? CurrentMaxVersion;
        public readonly int? MinVerForResponseErrorListV2; //any supported versions earlier than this number will use ResponseErrorList (v1) as per the CDS standards

        /// <summary>
        /// Constructs an option set where multiple versions of the endpoint are supported
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isXvMandatory"></param>
        /// <param name="minVersion"></param>
        /// <param name="maxVersion"></param>
        /// <param name="minVersionForErrorListV2"></param>
        public CdrApiEndpointVersionOptions(string path, bool isXvMandatory, int minVersion, int maxVersion, int minVersionForErrorListV2)
        {
            Path = path;
            IsVersioned = true;
            IsXVHeaderMandatory = isXvMandatory;
            CurrentMinVersion = minVersion;
            CurrentMaxVersion = maxVersion;
            MinVerForResponseErrorListV2 = minVersionForErrorListV2;
        }

        /// <summary>
        /// Constructs an option set where only one version of the endpoint is supported
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isXvMandatory"></param>
        /// <param name="version"></param>
        public CdrApiEndpointVersionOptions(string path, bool isXvMandatory, int version)
        {
            Path = path;
            IsVersioned = true;
            IsXVHeaderMandatory = isXvMandatory;
            CurrentMinVersion = version;
            CurrentMaxVersion = version;
            MinVerForResponseErrorListV2 = version;
        }

        /// <summary>
        /// Constructs an option set where the endpoint is unversioned
        /// </summary>
        /// <param name="path"></param>
        public CdrApiEndpointVersionOptions(string path)
        {
            Path = path;
            IsVersioned = false;
            IsXVHeaderMandatory = false;
        }
    }
}
