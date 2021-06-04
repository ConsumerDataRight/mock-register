namespace CDR.Register.Discovery.API.Business.Models
{
    public class EndpointDetailModel
    {
        public string Version { get; set; }
        public string PublicBaseUri { get; set; }
        public string ResourceBaseUri { get; set; }
        public string InfosecBaseUri { get; set; }
        public string ExtensionBaseUri { get; set; }
        public string WebsiteUri { get; set; }

    }
}