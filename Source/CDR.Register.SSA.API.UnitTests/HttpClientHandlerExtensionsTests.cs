using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

namespace CDR.Register.SSA.API.UnitTests
{
    public class HttpClientHandlerExtensionsTests
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClientHandler _handler = null!;

        public HttpClientHandlerExtensionsTests()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddInMemoryCollection(new Dictionary<string, string>()
                                {
                                    { "EnableServerCertificateValidation", "True"},
                                })
                                .Build();
            _configuration = configuration;

            _handler = new HttpClientHandler();
            _handler.SetServerCertificateValidation(_configuration);
        }

        [Theory]
        [InlineData("mock-data-recipient.pfx", "#M0ckDataRecipient#", true, "Valid TLS certificate provisioned by trusted CA should return true")]
        [InlineData("mock-data-recipient-invalid.pfx", "#M0ckDataRecipient#", false, "Expired TLS certificate should throw exception")]
        [InlineData("jwks.pfx", "#M0ckDataRecipient#", false, "Self-signed TLS certificate should throw exception")]
        public async Task ServerCertificates_ValidationEnabled_ShouldValidateSslConnection(
            string certName, string certPassword, bool expected, string reason)
        {
            await using (var mockEndpoint = new MockEndpoint("https://localhost:9990",
                Path.Combine(Directory.GetCurrentDirectory(), "Certificates", certName),
                certPassword))
            {
                mockEndpoint.Start();
                var client = new HttpClient(_handler);
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
            public MockEndpoint(string url, string certificatePath, string certificatePassword)
            {
                Url = url;
                CertificatePath = certificatePath;
                CertificatePassword = certificatePassword;
            }

            public string Url { get; init; }
            private int UrlPort => new Uri(Url).Port;
            public string CertificatePath { get; init; }
            public string CertificatePassword { get; init; }

            private IWebHost? _host;

            public void Start()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(Start), nameof(MockEndpoint));

                _host = new WebHostBuilder()
                    .UseKestrel(opts =>
                    {
                        opts.ListenAnyIP(UrlPort,
                            opts => opts.UseHttps(new X509Certificate2(CertificatePath, CertificatePassword, X509KeyStorageFlags.Exportable)));
                    })
                   .UseStartup(_ => new MockEndpointStartup())
                   .Build();

                _host.RunAsync();
            }

            public async Task Stop()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(Stop), nameof(MockEndpoint));

                if (_host != null)
                {
                    await _host.StopAsync();
                }
            }

            bool _disposed;
            public async ValueTask DisposeAsync()
            {
                Log.Information("Calling {FUNCTION} in {ClassName}.", nameof(DisposeAsync), nameof(MockEndpoint));

                if (!_disposed)
                {
                    await Stop();
                    _disposed = true;
                }

                GC.SuppressFinalize(this);
            }

            class MockEndpointStartup
            {
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
