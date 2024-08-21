namespace CDR.Register.API.Infrastructure.Models
{
    public class CdrSwaggerOptions
    {
        public string? SwaggerTitle { get; set; }

        public bool IncludeAuthentication { get; set; }

        public string VersionedApiGroupNameFormat { get; set; } = Constants.Versioning.GroupNameFormat; //default for group name format
    }
}
