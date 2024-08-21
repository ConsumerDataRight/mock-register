using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CDR.Register.IntegrationTests.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CDR.Register.IntegrationTests.Miscellaneous
{
    /// <summary>
    /// Integration tests for Register Admin API.
    /// </summary>   
    public class US12677_RegisterEnrolment_Tests : BaseTest
    {
        public US12677_RegisterEnrolment_Tests(ITestOutputHelper outputHelper, TestFixture testFixture) : base(outputHelper, testFixture) { }
        /// <summary>
        /// Get the repository as json
        /// </summary>
        private static async Task<HttpResponseMessage> GetJson()
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var client = new HttpClient(clientHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, ADMIN_URL);
            var response = await client.SendAsync(request);
            return response;
        }

        /// <summary>
        /// Load (replace) the repository from json
        /// </summary>
        private static async Task<HttpResponseMessage> PostJson(string json)
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var client = new HttpClient(clientHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, ADMIN_URL);
            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            var response = await client.SendAsync(request);
            return response;
        }

        [Fact]
        public async Task AC01_Get_ShouldRespondWith_200OK_RepositoryAsJson()
        {
            // Arrange            
            await TestFixture.Seeddata(); // TestFixture.InitializeAsync() seeds data but also then patches data in the database. Since we are just testing if import works need to import again (but without patching data).
            var json = await File.ReadAllTextAsync(SEEDDATA_FILENAME);
            var jToken = JToken.Parse(json);

            // Act            
            var response = await GetJson();
            var responseJson = await response.Content.ReadAsStringAsync();
            var jTokenResponse = JToken.Parse(responseJson);

            jToken = Cleanup(jToken);
            jTokenResponse = Cleanup(jTokenResponse);

            // Assert
            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                Assert_HasContentType_ApplicationJson(response.Content);

                JToken.DeepEquals(jToken, jTokenResponse).Should().BeTrue();
            }
        }

        [Fact]
        public async Task AC02_Post_WithEmptyRequestBody_ShouldRespondWith_400BadRequest()
        {
            // Arrange

            // Act
            var response = await PostJson(null);

            // Assert
            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                Assert_HasContentType_ApplicationJson(response.Content);
            }
        }

        [Fact]
        public async Task AC03_Post_WithEmptyJson_ShouldRespondWith_400BadRequest()
        {
            // Arrange

            // Act
            var response = await PostJson("{}");

            // Assert
            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                Assert_HasContentType_ApplicationJson(response.Content);
            }
        }

        [Fact]
        public async Task AC04_Post_WithInvalidJson_ShouldRespondWith_400BadRequest()
        {
            // Arrange

            // Act
            var response = await PostJson(@"{ ""foo"": ""bar"" }");

            // Assert
            using (new AssertionScope())
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                Assert_HasContentType_ApplicationJson(response.Content);
            }
        }

        // Remove "foreign keys" (LegalEntityId, ParticipationId, etc) from child collections as their values can be dynamic.
        private static void RemoveForeignKeys(JToken jToken)
        {
            static void ClearField(JToken jToken, string fieldName)
            {
                foreach (var item in jToken.SelectTokens($"..{fieldName}"))
                {
                    var prop = item as JValue;
                    if (prop != null) prop.Value = null;
                }
            }

            ClearField(jToken, "legalEntityId");
            ClearField(jToken, "participationId");
            ClearField(jToken, "brandId");
            ClearField(jToken, "softwareProductId");
            ClearField(jToken, "softwareProductCertificateId");

            ClearField(jToken, "legalEntityStatusId");
        }

        static JToken Cleanup(JToken jToken)
        {
            // Sort
            jToken["legalEntities"].SortArray("legalEntityId");
            jToken.SortArray("$..legalEntities..participations", "participationId");
            jToken.SortArray("$..legalEntities..participations..brands", "brandId");
            jToken.SortArray("$..legalEntities..participations..brands..softwareProducts", "softwareProductId");

            RemoveForeignKeys(jToken);

            jToken.RemoveNulls();
            jToken.RemoveEmptyArrays();

            // Fix mismatch in version, convert to string
            jToken.ReplacePath("$..legalEntities..participations..brands..endpoint..version",
                t => $"{t}");

            // Issues with Guids and property names having different cases... not ideal, but just convert to upper case
            jToken = JToken.Parse(jToken.ToString().ToUpper());

            return jToken;
        }

        [Fact]
        public async Task AC05_AC06_Post_WithValidJson_ThenGet_ShouldRespondWith_200OK_ReturnedJsonMatchesPostedJson()
        {
            // Arrange
            var json = await File.ReadAllTextAsync(SEEDDATA_FILENAME);
            var jToken = JToken.Parse(json);

            // Act
            var postResponse = await PostJson(json);
            var getResponse = await GetJson();

            var responseJson = await getResponse.Content.ReadAsStringAsync();
            var jTokenResponse = JToken.Parse(responseJson);

            // Clean up json data for comparison.
            jToken = Cleanup(jToken);
            jTokenResponse = Cleanup(jTokenResponse);

            // Assert
            using (new AssertionScope())
            {
                postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                Assert_HasContentType_ApplicationJson(getResponse.Content);

                JToken.DeepEquals(jToken, jTokenResponse).Should().BeTrue();
            }
        }

        [Fact]
        public async Task AC07_AC08_Get_ThenPostUpdatedData_ShouldRespondWith_200OK_ReturnedJsonMatchesPostedJson()
        {
            static void UpdateNames(JToken jToken)
            {
                if (jToken["legalEntities"] != null)
                    foreach (var legalEntity in jToken["legalEntities"])
                    {
                        legalEntity["legalEntityName"] += " updated";

                        if (legalEntity["participations"] != null)
                            foreach (var participation in legalEntity["participations"])
                            {
                                foreach (var brand in participation["brands"])
                                {
                                    brand["brandName"] += " updated";

                                    if (brand["softwareProducts"] != null)
                                        foreach (var softwareProduct in brand["softwareProducts"])
                                        {
                                            softwareProduct["softwareProductName"] += " updated";
                                        }
                                }
                            }
                    }
            }

            // Arrange
            var json = await (await GetJson()).Content.ReadAsStringAsync();  // get json from Admin endpoint
            var jToken = JToken.Parse(json);

            UpdateNames(jToken); // append "updated" to names
            json = jToken.ToString();

            // Act
            var postResponse = await PostJson(json);
            var getResponse = await GetJson();

            var responseJson = await getResponse.Content.ReadAsStringAsync();
            var jTokenResponse = JToken.Parse(responseJson);

            jToken = Cleanup(jToken);
            jTokenResponse = Cleanup(jTokenResponse);

            // Assert
            using (new AssertionScope())
            {
                postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                Assert_HasContentType_ApplicationJson(getResponse.Content);

                JToken.DeepEquals(jToken, jTokenResponse).Should().BeTrue();
            }
        }
    }
}
