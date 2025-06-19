using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

#nullable enable

namespace CDR.Register.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Call API.
    /// </summary>
    public class Api
    {
        private const string CERTIFICATE_THUMBPRINT_HEADER_NAME = "X-SSLClientCertThumbprint";
        private const string CERTIFICATE_CN_HEADER_NAME = "X-SSLClientCertCN";

        /// <summary>
        /// Gets or sets Filename of certificate to use.
        /// If null then no certificate will be attached to the request.
        /// </summary>
        public string? CertificateFilename { get; set; }

        /// <summary>
        /// Gets or sets Password for certificate.
        /// If null then no certificate password will be set.
        /// </summary>
        public string? CertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets Access token.
        /// If null then no access token will be attached to the request.
        /// See the AccessToken class to generate an access token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the HttpMethod of the request.
        /// </summary>
        public HttpMethod? HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets URL of the request.
        /// </summary>
        public string? URL { get; set; }

        /// <summary>
        /// Gets or sets x-v header.
        /// If null then no x-v header will be set.
        /// </summary>
        public string? XV { get; set; }

        /// <summary>
        /// Gets or sets  x-min-v header.
        /// If null then no x-min-v header will be set.
        /// </summary>
        public string? XMinV { get; set; }

        /// <summary>
        /// Gets or sets  If-None-Match header (an ETag).
        /// If null then no If-None-Match header will be set.
        /// </summary>
        public string? IfNoneMatch { get; set; }

        public string? CertificateThumbprint { get; set; } = null;

        public string? CertificateCn { get; set; } = null;

        /// <summary>
        /// Send a request to the API.
        /// </summary>
        /// <returns>The API response.</returns>
        public async Task<HttpResponseMessage> SendAsync()
        {
            // Build request
            HttpRequestMessage BuildRequest()
            {
                if (this.HttpMethod == null)
                {
                    throw new Exception($"{nameof(Api)}.{nameof(this.SendAsync)}.{nameof(BuildRequest)} - {nameof(this.HttpMethod)} not set");
                }

                var request = new HttpRequestMessage(this.HttpMethod, this.URL);

                // Attach access token if provided
                if (this.AccessToken != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, this.AccessToken);
                }

                // Set x-v header if provided
                if (this.XV != null)
                {
                    request.Headers.Add("x-v", this.XV);
                }

                // Set x-min-v header if provided
                if (this.XMinV != null)
                {
                    request.Headers.Add("x-min-v", this.XMinV);
                }

                // Set If-None-Match header if provided
                if (this.IfNoneMatch != null)
                {
                    request.Headers.Add("If-None-Match", $"\"{this.IfNoneMatch}\"");
                }

                if (this.CertificateThumbprint != null && this.CertificateCn != null)
                {
                    request.Headers.Add(CERTIFICATE_THUMBPRINT_HEADER_NAME, this.CertificateThumbprint);
                    request.Headers.Add(CERTIFICATE_CN_HEADER_NAME, this.CertificateCn);
                }

                return request;
            }

            // Send request and return response
            async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
            {
                HttpClient GetClient()
                {
                    // Attach client certificate
                    if (this.CertificateFilename != null)
                    {
                        var clientHandler = new HttpClientHandler();

                        clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                        clientHandler.ClientCertificates.Add(new X509Certificate2(
                            this.CertificateFilename,
                            this.CertificatePassword,
                            X509KeyStorageFlags.Exportable));

                        return new HttpClient(clientHandler);
                    }
                    else
                    {
                        var clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                        return new HttpClient(clientHandler);
                    }
                }

                using var client = GetClient();

                var response = await client.SendAsync(request);

                return response;
            }

            var request = BuildRequest();
            var response = await SendRequest(request);
            return response;
        }
    }
}
