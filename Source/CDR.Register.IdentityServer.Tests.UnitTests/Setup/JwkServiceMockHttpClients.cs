using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public static class JwkServiceMockHttpClients
    {
        public static HttpClient HttpClientWithResponseMessageNotFound()
        {
            return new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("This is not found")
                };

                return await Task.FromResult(responseMessage);
            }));
        }

        public static HttpClient HttpClientWithResponseMessageBadRequest()
        {
            return new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("This is a bad request")
                };

                return await Task.FromResult(responseMessage);
            }));
        }

        public static HttpClient HttpClientWithResponseMessageOK_Contains_JWKS_Content()
        {
            return new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"keys\": [ {\"e\":\"AQAB\",\"kid\":\"EC555BDBAAE6458B9B93098AACA0F81F\",\"kty\":\"RSA\",\"n\":\"vPS0EyZJEHWc9q44P73uTsDSBjSqRYmT8kYMlPRlpqruUQ8d2wkVHBi9x_iI061ZBwNSbfgXE8SzCiGMD8rz_S1bQqw4Q0vZe-URHm1NlzmOKaKtgaOOuU1ztybuW1qWBb2hmHDh2msmaubn5GYsAikCPSjqAG0ZcpW1MUAtdrX6U351wm5oN_yJFtchhsvgHJY0XTSl7u1kbfe-Uhy2qU6Hd7xdSpo9wCWjz8EcOHmkMvLBHbWrhe_ZFD4UvIyycBKYvtyxdqE8Chhq3aCfL5urEhwmgQ-TIj9qGcnRDp--uUFtgEF6FWeqeGDXqKWglzrc-ea7UMm1FUEYr2ZmgQ\"}]}")
                };

                return await Task.FromResult(responseMessage);
            }));
        }

        public static HttpClient HttpClientWithResponseMessageOK_No_JWKS_Content()
        {
            return new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"nokeys\":\"\"}")
                };

                return await Task.FromResult(responseMessage);
            }));
        }
    }
}
