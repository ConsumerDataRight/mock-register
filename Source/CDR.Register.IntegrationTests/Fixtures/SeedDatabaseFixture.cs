using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Extensions;
using Xunit;

namespace CDR.Register.IntegrationTests.Fixtures
{
    public class SeedDatabaseFixture : IAsyncLifetime
    {
        // Ensure database is in known state prior to running tests by seeding it
        public async Task InitializeAsync()
        {
            string jsonFromSeedDataFile =
                (await File.ReadAllTextAsync(BaseTest.SEEDDATA_FILENAME))
                .JsonStripComments();

            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var client = new HttpClient(clientHandler);

            var request = new HttpRequestMessage(HttpMethod.Post, BaseTest.ADMIN_URL);
            request.Content = new StringContent(jsonFromSeedDataFile, Encoding.UTF8, "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error seeding database");
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
