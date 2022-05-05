using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CDR.Register.Domain.Repositories;
using CDR.Register.IdentityServer.Configurations;
using CDR.Register.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Services;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using IdentityServer4.ResponseHandling;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CDR.Register.IdentityServer
{

    public static class IdSvrExtensions
    {
        public static IServiceCollection AddRegisterIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IJwkService, JwkService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                    };

                    return handler;
                });
            services.AddScoped<IClientService, ClientService>();
            services.AddTransient<IDiscoveryResponseGenerator, RegisterDiscoveryResponseGenerator>();

            var issuerUri = configuration[Constants.ConfigurationKeys.IssuerUri];
            var jwksUri = configuration[Constants.ConfigurationKeys.JwksUri];
            var tokenUri = configuration[Constants.ConfigurationKeys.TokenUri];

            services.AddIdentityServer(options =>
            {
                if (!string.IsNullOrWhiteSpace(issuerUri))
                {
                    options.IssuerUri = issuerUri;
                }

                if (!string.IsNullOrWhiteSpace(jwksUri))
                {
                    options.Discovery.CustomEntries.Add(Constants.DiscoveryOverrideKeys.JwksUri, jwksUri);
                }

                if (!string.IsNullOrWhiteSpace(tokenUri))
                {
                    options.Discovery.CustomEntries.Add(Constants.DiscoveryOverrideKeys.TokenEndpoint, tokenUri);
                }

                // Raise events so we can log them with the LoggingEventSink
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;

                // Disable endpoints and entries that are not required for the CDR register
                options.Endpoints.EnableAuthorizeEndpoint = false;
                options.Endpoints.EnableCheckSessionEndpoint = false;
                options.Endpoints.EnableDeviceAuthorizationEndpoint = false;
                options.Endpoints.EnableEndSessionEndpoint = false;
                options.Endpoints.EnableIntrospectionEndpoint = false;
                options.Endpoints.EnableJwtRequestUri = false;
                options.Endpoints.EnableTokenRevocationEndpoint = false;
                options.Endpoints.EnableUserInfoEndpoint = false;

                options.Discovery.ShowClaims = true;
                options.Discovery.ShowResponseModes = false;
                options.Discovery.ShowIdentityScopes = false;
                options.Discovery.ShowExtensionGrantTypes = false;

                // Override default entries with custom entries to only show relevant otions for the CDR register
                options.Discovery.ShowApiScopes = false;
                options.Discovery.CustomEntries.Add("scopes_supported", new string[] { CdsRegistrationScopes.BankRead, CdsRegistrationScopes.Read });

                options.Discovery.ShowResponseTypes = false;
                options.Discovery.CustomEntries.Add("response_types_supported", new string[] { "token" });

                options.Discovery.ShowGrantTypes = false;
                options.Discovery.CustomEntries.Add("grant_types_supported", new string[] { "client_credentials" });

                options.Discovery.ShowTokenEndpointAuthenticationMethods = false;
                options.Discovery.CustomEntries.Add("token_endpoint_auth_methods_supported", new string[] { "private_key_jwt" });

                options.MutualTls.Enabled = false;
                options.Discovery.CustomEntries.Add("tls_client_certificate_bound_access_tokens", true);

                options.Discovery.CustomEntries.Add("token_endpoint_auth_signing_alg_values_supported", new string[] { "PS256" });
            })
                .AddSecretParser<CdrSecretParser>()
                .AddInMemoryApiResources(InMemoryConfig.Apis)
                .AddInMemoryIdentityResources(InMemoryConfig.IdentityResources)
                .AddInMemoryApiScopes(InMemoryConfig.Scopes)
                .AddRegisterSigningCredential(configuration)
                .AddSecretValidator<CdrSecretValidator>()
                .AddClientStore<CdrClientStore>();

            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);

            services.AddRegisterIdSvrDatabase(configuration);

            return services;
        }

        private static void AddRegisterIdSvrDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IRepositoryMapper, RepositoryMapper>();
            services.AddScoped<IRegisterIdSvrRepository, RegisterIdSvrRepository>();

            services.AddDbContext<RegisterDatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("Register_DB")));
        }

        private static IIdentityServerBuilder AddRegisterSigningCredential(this IIdentityServerBuilder builder, IConfiguration configuration)
        {
            var filePath = configuration["SigningCertificate:Path"];
            var pwd = configuration["SigningCertificate:Password"];
            var cert = new X509Certificate2(filePath, pwd);
            var certificateVersionSecurityKey = new X509SecurityKey(cert);
            var credentials = new SigningCredentials(certificateVersionSecurityKey, SecurityAlgorithms.RsaSsaPssSha256);

            builder.AddSigningCredential(credentials);

            return builder;
        }

    }
}
