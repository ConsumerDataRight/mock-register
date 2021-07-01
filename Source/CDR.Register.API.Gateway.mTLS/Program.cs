using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CDR.Register.API.Gateway.mTLS
{
    public static class Program
    {

        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                            .BuildRegisterConfiguration(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args, configuration).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host
                .CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddConfiguration(configuration);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel((context, serverOptions) =>
                        {
                            serverOptions.Configure(context.Configuration.GetSection("Kestrel"))
                                .Endpoint("HTTPS", listenOptions =>
                                {
                                    listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                                });
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .ConfigureKestrel(o =>
                        {
                            o.ConfigureHttpsDefaults(listenOptions =>
                            {
                                listenOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

                                // On non-Windows platform the CipherSuitesPolicy can be set.
                                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                                {
                                    listenOptions.OnAuthenticate = (context, sslOptions) =>
                                    {
                                        // Set the cipher suites dictated by the CDS.
                                        sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(
                                            new[] {
                                            TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                                            TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
                                            });
                                    };
                                }
                            });
                        })
                        .UseStartup<Startup>();
                });
    }
}
