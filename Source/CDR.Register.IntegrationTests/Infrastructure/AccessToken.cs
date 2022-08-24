using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

namespace CDR.Register.IntegrationTests.Infrastructure
{
    public class AccessToken
    {
        private static readonly string IDENTITYSERVER_URL = BaseTest.IDENTITYSERVER_URL;
        private static readonly string AUDIENCE = IDENTITYSERVER_URL;
        private const string SCOPE = "cdr-register:bank:read cdr-register:read";
        private const string GRANT_TYPE = "client_credentials";
        private const string CLIENT_ID = "86ecb655-9eba-409c-9be3-59e7adf7080d";
        private const string CLIENT_ASSERTION_TYPE = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        private const string ISSUER = CLIENT_ID;

        /// <summary>
        /// Filename of certificate to use. 
        /// If null then no certificate will be attached to the request.
        /// </summary>
        public string? CertificateFilename { get; set; }

        /// <summary>
        /// Password for certificate. 
        /// If null then no certificate password will be set.
        /// </summary>
        public string? CertificatePassword { get; set; }

        public string Issuer { get; set; } = ISSUER;
        public string Audience { get; set; } = AUDIENCE;
        public string Scope { get; set; } = SCOPE;
        public string GrantType { get; set; } = GRANT_TYPE;
        public string ClientId { get; set; } = CLIENT_ID;
        public string ClientAssertionType { get; set; } = CLIENT_ASSERTION_TYPE;

        /// <summary>
        /// Get HttpRequestMessage for access token request
        /// </summary>
        private static HttpRequestMessage CreateAccessTokenRequest(
           string? certificateFilename, string? certificatePassword,
           string issuer, string audience,
           string scope, string grant_type, string client_id, string client_assertion_type)
        {
            static string BuildContent(string scope, string grant_type, string client_id, string client_assertion_type, string client_assertion)
            {
                var kvp = new KeyValuePairBuilder();

                if (scope != null)
                {
                    kvp.Add("scope", scope);
                }

                if (grant_type != null)
                {
                    kvp.Add("grant_type", grant_type);
                }

                if (client_id != null)
                {
                    kvp.Add("client_id", client_id);
                }

                if (client_assertion_type != null)
                {
                    kvp.Add("client_assertion_type", client_assertion_type);
                }

                if (client_assertion != null)
                {
                    kvp.Add("client_assertion", client_assertion);
                }

                return kvp.Value;
            }

            var tokenizer = new PrivateKeyJwt(certificateFilename, certificatePassword);
            var client_assertion = tokenizer.Generate(issuer, audience);

            var request = new HttpRequestMessage(HttpMethod.Post, IDENTITYSERVER_URL)
            {
                Content = new StringContent(
                    BuildContent(scope, grant_type, client_id, client_assertion_type, client_assertion),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            return request;
        }

        /// <summary>
        /// Get an access token from Identity Server
        /// </summary>
        public async Task<string?> GetAsync()
        {
            // Create ClientHandler 
            var _clientHandler = new HttpClientHandler();
            _clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Attach client certificate to handler
            if (CertificateFilename != null)
            {
                var clientCertificate = new X509Certificate2(CertificateFilename, CertificatePassword, X509KeyStorageFlags.Exportable);
                _clientHandler.ClientCertificates.Add(clientCertificate);
            }

            // Create HttpClient
            using var client = new HttpClient(_clientHandler);

            // Create an access token request
            var request = CreateAccessTokenRequest(
                CertificateFilename, CertificatePassword,
                Issuer, Audience,
                Scope, GrantType, ClientId, ClientAssertionType);

            // Request the access token
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK) 
            {
                throw new Exception($"{nameof(AccessToken)}.{nameof(GetAsync)} - Error getting access token");
            }

            // Deserialize the access token from the response
            var accessToken = JsonSerializer.Deserialize<Models.AccessToken>(await response.Content.ReadAsStringAsync());

            // And return the access token
            return accessToken?.access_token;
        }
    }
}
