using Newtonsoft.Json;

namespace CDR.Register.Infosec.Models
{    
    public class DiscoveryDocument
    {
        [JsonProperty("issuer")]
        public string? Issuer { get; set; }

        [JsonProperty("jwks_uri")]
        public string? JwksUri { get; set; }

        [JsonProperty("token_endpoint")]
        public string? TokenEndpoint { get; set; }

        [JsonProperty("claims_supported")]
        public string[]? ClaimsSupported { get; set; }

        [JsonProperty("id_token_signing_alg_values_supported")]
        public string[]? IdTokenSigningAlgValuesSupported { get; set; }

        [JsonProperty("subject_types_supported")]
        public string[]? SubjectTypesSupported { get; set; }

        [JsonProperty("scopes_supported")]
        public string[]? ScopesSupported { get; set; }

        [JsonProperty("code_challenge_methods_supported")]
        public string[]? CodeChallengeMethodsSupported { get; set; }

        [JsonProperty("response_types_supported")]
        public string[]? ResponseTypesSupported { get; set; }

        [JsonProperty("grant_types_supported")]
        public string[]? GrantTypesSupported { get; set; }

        [JsonProperty("token_endpoint_auth_methods_supported")]
        public string[]? TokenEndpointAuthMethodsSupported { get; set; }

        [JsonProperty("tls_client_certificate_bound_access_tokens")]
        public bool TlsClientCertificateBoundAccessTokens { get; set; }

        [JsonProperty("token_endpoint_auth_signing_alg_values_supported")]
        public string[]? TokenEndpointAuthSigningAlgValuesSupported { get; set; }

    }
}
