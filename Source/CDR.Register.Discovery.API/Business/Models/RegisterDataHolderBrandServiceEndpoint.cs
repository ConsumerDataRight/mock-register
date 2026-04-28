using System;

namespace CDR.Register.Discovery.API.Business.Models
{
    [Obsolete("Deprecated in the standards, used by versions prior to V1.35.0. This is aligned with RAAP implementation and can be removed when the endpoint is no longer supported.", false)]
    public class RegisterDataHolderBrandServiceEndpoint
    {
        public string Version { get; set; }

        public string PublicBaseUri { get; set; }

        public string ResourceBaseUri { get; set; }

        public string InfosecBaseUri { get; set; }

        public string ExtensionBaseUri { get; set; }

        public string WebsiteUri { get; set; }
    }
}
