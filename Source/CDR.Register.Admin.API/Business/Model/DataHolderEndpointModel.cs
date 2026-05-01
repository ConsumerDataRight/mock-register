namespace CDR.Register.Admin.API.Business.Model
{
    public class DataHolderEndpointModel
    {
        public string Version { get; set; } = string.Empty;

        public string PublicBaseUri { get; set; } = string.Empty;

        public string ResourceBaseUri { get; set; } = string.Empty;

        public string InfosecBaseUri { get; set; } = string.Empty;

        public string? ExtensionBaseUri { get; set; }

        public string WebsiteUri { get; set; } = string.Empty;
    }
}
