using CDR.Register.IntegrationTests.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    public class TestFixture : IAsyncLifetime
    {
        // Seed register data
        public static async Task Seeddata()
        {
            try
            {
                string jsonFromSeedDataFile = (await File.ReadAllTextAsync(BaseTest.SEEDDATA_FILENAME)).JsonStripComments();

                var clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                var client = new HttpClient(clientHandler);

                var request = new HttpRequestMessage(HttpMethod.Post, BaseTest.ADMIN_URL);
                request.Content = new StringContent(jsonFromSeedDataFile, Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error seeding database - HttpStatusCode {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seeding database - {ex.Message}");
            }
        }

        // Patch JWKSURI to be the Register loopback URI
        private static async Task PatchRegister()
        {
            using var connection = new SqlConnection(BaseTest.Configuration.GetConnectionString("DefaultConnection"));

            await connection.OpenAsync();

            using var updateCommand = new SqlCommand($@"update softwareproduct set jwksuri = '{BaseTest.ADMIN_BaseURL}/loopback/MockDataRecipientJwks'", connection);

            await updateCommand.ExecuteNonQueryAsync();
        }

        public async Task InitializeAsync()
        {
            await Seeddata();
            await PatchRegister();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}