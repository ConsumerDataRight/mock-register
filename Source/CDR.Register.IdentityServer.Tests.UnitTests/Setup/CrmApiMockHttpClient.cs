using CDR.Register.IdentityServer.Tests.Common.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public static class CrmApiMockHttpClient
    {
        private static readonly Uri _baseAddress = new Uri("https://localhost/raap/api/data/v9.0/");

        public static HttpClient HttpClient_DataRecipientBrand()
        {
            HttpClient mockHttpClient = new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var dataFolderPath = TestEnvironmentHelper.GetDataFolderPath();
                string mockResponseContent = string.Empty;

                if (request.RequestUri.ToString().Contains(@"api/data/v9.0/cdr_cdrregisters"))
                {
                    mockResponseContent = File.ReadAllText(Path.Combine(dataFolderPath, "MockDataRecipientsCrmQueryResponse.json"));
                }

                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(mockResponseContent)
                };

                return await Task.FromResult(responseMessage);
            }));

            mockHttpClient.BaseAddress = _baseAddress;

            return mockHttpClient;
        }

        public static HttpClient HttpClient_DataNoBrandOnlyProduct(bool inactiveProduct = false)
        {
            HttpClient mockHttpClient = new HttpClient(new MockHttpMessageHandler(async (request, cancellationToken) =>
            {
                var dataFolderPath = TestEnvironmentHelper.GetDataFolderPath();
                string mockResponseContent = string.Empty;
                var status = HttpStatusCode.OK;
                if (request.RequestUri.ToString().Contains(@"api/data/v9.0/cdr_cdrregisters"))
                {
                    status = HttpStatusCode.NotFound;
                }
                else if (request.RequestUri.ToString().Contains(@"api/data/v9.0/cdr_participantproducts"))
                {
                    if (request.RequestUri.ToString().Contains(@"cdr_provider"))
                        mockResponseContent = File.ReadAllText(Path.Combine(dataFolderPath, inactiveProduct ? "MockDataSoftwareProductsInactiveProviderCrmQueryResponse.json" : "MockDataSoftwareProductsCrmQueryResponse.json"));
                }

                var responseMessage = new HttpResponseMessage(status)
                {
                    Content = new StringContent(mockResponseContent)
                };

                return await Task.FromResult(responseMessage);
            }));

            mockHttpClient.BaseAddress = _baseAddress;

            return mockHttpClient;
        }
    }
}
