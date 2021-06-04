using CDR.Register.API.IdentityServer.Interfaces;
using CDR.Register.IdentityServer.Services;
using CDR.Register.IdentityServer.Tests.UnitTests.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CDR.Register.IdentityServer.Tests.IntegrationTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void AddConfigEntryExt(IServiceCollection services, IKeyVaultService keyVaultService)
        {
            services.AddSingleton<ConfigEntry>(ConfigEntryMock.GetMockConfigEntry());
        }
    }
}
