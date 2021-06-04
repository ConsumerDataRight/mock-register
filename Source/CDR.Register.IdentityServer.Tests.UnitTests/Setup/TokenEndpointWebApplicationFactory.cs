using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Tests.IntegrationTests;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public class TokenEndpointWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                              .ConfigureWebHostDefaults(x =>
                              {
                                  x.UseStartup<TestStartup>();
                              });
            return builder;
        }

        public string GetContentRootPath()
        {
            var testProjectPath = AppContext.BaseDirectory;
            var relativePathToHostProject = @"..\..\..\..\..\CDR.Register.IdentityServer";
            return Path.Combine(testProjectPath, relativePathToHostProject);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                // for debugging
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var keyVaultUrl = configuration.GetValue<string>("KeyVaultUrl");
                Trace.Write("keyVaultUrl from configuration " + keyVaultUrl);
                Console.WriteLine("keyVaultUrl from configuration " + keyVaultUrl);

                var descriptorsToBeRemoved = services.Where(s =>
                    s.ServiceType == typeof(IJwkService) ||
                    s.ServiceType == typeof(IClientStore) ||
                    s.ServiceType == typeof(ISecretParser) ||
                    s.ServiceType == typeof(ISecretValidator) ||
                    s.ServiceType == typeof(IMediator) ||
                    s.ServiceType == typeof(ISigningCredentialStore) ||
                    s.ServiceType == typeof(IValidationKeysStore));

                for (int i = 0; i < descriptorsToBeRemoved.Count(); i++)
                {
                    services.Remove(descriptorsToBeRemoved.ElementAt(i));
                }

                // Note: AzureDevops build pipeline service account cannot access Azure Keyvault, 
                // so change ISigningCredentialStore and IValidationKeysStore here to pass build pipeline for unit tests
                var credential = JwkMock.GetMockSingleCredential();
                services.AddSingleton<ISigningCredentialStore>(new DefaultSigningCredentialsStore(credential));

                var keyInfo = new SecurityKeyInfo
                {
                    Key = credential.Key,
                    SigningAlgorithm = credential.Algorithm,
                };
                services.AddSingleton<IValidationKeysStore>(new DefaultValidationKeysStore(new[] { keyInfo }));

            });
        }
    }
}
