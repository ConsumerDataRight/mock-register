using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _sendAsync(request, cancellationToken);
        }
    }
}
