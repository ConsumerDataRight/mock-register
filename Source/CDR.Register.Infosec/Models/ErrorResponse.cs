using Newtonsoft.Json;

namespace CDR.Register.Infosec.Models
{
    public class ErrorResponse : IResponseBase
    {
        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("error_description")]
        public string? ErrorDescription { get; set; }
    }
}
