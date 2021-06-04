using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace CDR.Register.IdentityServer.Configurations
{
    public class RegisterDiscoveryResponseGenerator : DiscoveryResponseGenerator
    {

        public RegisterDiscoveryResponseGenerator(
            IdentityServerOptions options,
            IResourceStore resourceStore,
            IKeyMaterialService keys,
            ExtensionGrantValidator extensionGrants,
            ISecretsListParser secretParsers,
            IResourceOwnerPasswordValidator resourceOwnerValidator,
            ILogger<DiscoveryResponseGenerator> logger) : base(options, resourceStore, keys, extensionGrants, secretParsers, resourceOwnerValidator, logger)
        { }

        public async override Task<Dictionary<string, object>> CreateDiscoveryDocumentAsync(string baseUrl, string issuerUri)
        {
            var oidc = await base.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);

            object jwksUri = null;
            if (Options.Discovery.CustomEntries.TryGetValue(Constants.DiscoveryOverrideKeys.JwksUri, out jwksUri))
            {
                oidc["jwks_uri"] = jwksUri;
                oidc.Remove(Constants.DiscoveryOverrideKeys.JwksUri);
            }

            object tokenUri = null;
            if (Options.Discovery.CustomEntries.TryGetValue(Constants.DiscoveryOverrideKeys.TokenEndpoint, out tokenUri))
            {
                oidc["token_endpoint"] = tokenUri;
                oidc.Remove(Constants.DiscoveryOverrideKeys.TokenEndpoint);
            }

            return oidc;
        }
    }
}
