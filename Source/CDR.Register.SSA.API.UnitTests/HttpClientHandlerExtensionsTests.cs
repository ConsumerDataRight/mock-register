using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

#nullable enable

namespace CDR.Register.SSA.API.UnitTests
{
    public class HttpClientHandlerExtensionsTests
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClientHandler _handler;

        public HttpClientHandlerExtensionsTests()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddInMemoryCollection(new Dictionary<string, string?>()
                                {
                                    { "EnableServerCertificateValidation", "True" },
                                })
                                .Build();
            this._configuration = configuration;

            this._handler = new HttpClientHandler();
            this._handler.SetServerCertificateValidation(this._configuration);
        }

        [Theory]
        [InlineData("mock-data-recipient.pfx", "#M0ckDataRecipient#", true, "Valid TLS certificate provisioned by trusted CA should return true")]
        [InlineData("mock-data-recipient-invalid.pfx", "#M0ckDataRecipient#", false, "Expired TLS certificate should throw exception")]
        [InlineData("jwks.pfx", "#M0ckDataRecipient#", false, "Self-signed TLS certificate should throw exception")]
        public async Task ServerCertificates_ValidationEnabled_ShouldValidateSslConnection(
            string certName, string certPassword, bool expected, string reason)
        {
            Log.Information($"Scenario: {reason}");

            await using (var mockEndpoint = new MockEndpoint(
                "https://localhost:9990",
                Path.Combine(Directory.GetCurrentDirectory(), "Certificates", certName),
                certPassword))
            {
                mockEndpoint.Start();
                var client = new HttpClient(this._handler);
                if (expected)
                {
                    var result = await client.GetAsync("https://localhost:9990");
                    Assert.NotNull(result);
                }
                else
                {
                    await Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetAsync("https://localhost:9990"));
                }

                await mockEndpoint.Stop();
            }
        }

        public partial class MockEndpoint : IAsyncDisposable
        {
            private IWebHost? _host;
            private bool _disposed;

            public MockEndpoint(string url, string certificatePath, string certificatePassword)
            {
                this.Url = url;
                this.CertificatePath = certificatePath;
                this.CertificatePassword = certificatePassword;
            }

            public string Url { get; init; }

            public string CertificatePath { get; init; }

            public string CertificatePassword { get; init; }

            private int UrlPort => new Uri(this.Url).Port;

            public void Start()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(this.Start), nameof(MockEndpoint));

                this._host = new WebHostBuilder()
                    .UseKestrel(opts =>
                    {
                        opts.ListenAnyIP(
                            this.UrlPort,
                            opts => opts.UseHttps(new X509Certificate2(this.CertificatePath, this.CertificatePassword, X509KeyStorageFlags.Exportable)));
                    })
                   .UseStartup(typeof(MockEndpointStartup))
                   .Build();

                this._host.RunAsync();
            }

            public async Task Stop()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(this.Stop), nameof(MockEndpoint));

                if (this._host != null)
                {
                    await this._host.StopAsync();
                }
            }

            public async ValueTask DisposeAsync()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(this.DisposeAsync), nameof(MockEndpoint));

                if (!this._disposed)
                {
                    await this.Stop();
                    this._disposed = true;
                }

                GC.SuppressFinalize(this);
            }

            public class MockEndpointStartup
            {
                protected MockEndpointStartup()
                {
                }

                public static void Configure(IApplicationBuilder app)
                {
                    app.UseHttpsRedirection();
                    app.UseRouting();
                }

                public static void ConfigureServices(IServiceCollection services)
                {
                    services.AddRouting();
                }
            }
        }
    }
}
