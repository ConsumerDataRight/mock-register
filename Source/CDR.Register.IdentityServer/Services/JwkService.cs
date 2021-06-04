using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CDR.Register.IdentityServer.Services
{
    public class JwkService : IJwkService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JwkService> _logger;
        private readonly IMediator _mediator;

        public JwkService(ILogger<JwkService> logger, HttpClient httpClient, IMediator mediator)
        {
            _logger = logger;
            _httpClient = httpClient;
            _mediator = mediator;
        }

        public async Task<IList<JsonWebKey>> GetJwksAsync(string jwksUrl)
        {
            var httpResponse = await _httpClient.GetAsync(jwksUrl);

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogDebug($"Http response body:\r\n{responseContent}");

            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                await _mediator.Publish(new NotificationMessage(GetType().Name, "801", null,
                    $"{jwksUrl} returned 404."));
                return null;
            }
            else if (!httpResponse.IsSuccessStatusCode)
            {
                await _mediator.Publish(new NotificationMessage(GetType().Name, "803", null,
                    $"{jwksUrl} returned {httpResponse.StatusCode} Content:\r\n{responseContent}"));
                throw new HttpRequestException($"Http Status Code: {httpResponse.StatusCode} Content:\r\n{responseContent}");
            }

            var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseContent);
            if (keys == null || keys.Keys == null || keys.Keys.Count == 0)
            {
                await _mediator.Publish(new NotificationMessage(GetType().Name, "802", null,
                    $"No JWKS found from {jwksUrl}."));
                return null;
            }

            await _mediator.Publish(new NotificationMessage(GetType().Name, "0", null));

            return keys.Keys;
        }
    }
}
