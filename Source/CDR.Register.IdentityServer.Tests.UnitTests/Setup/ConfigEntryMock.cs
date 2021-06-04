using CDR.Register.IdentityServer.Services;

namespace CDR.Register.IdentityServer.Tests.UnitTests.Setup
{
    public static class ConfigEntryMock
    {
        public static ConfigEntry GetMockConfigEntry()
        {
            return new ConfigEntry()
            {
                CrmApiBaseUri = @"https://localhost/raap/api/data/v9.0/",
                oAuthClientId = "fake_oAuthClientId",
                oAuthTokenEndpoint = @"https://localhost/connect/token",
                oAuthClientSecretName = "fake_oAuthClientSecretName",
                oAuthClientSecret = "fake_oAuthClientSecret",
                UserName = "fake_UserName",
                PasswordSecretName = "fake_PasswordSecretName",
                Password = "fake_Password",
                StorageName = "fake_StorageName",
                StorageKey = "fake_StorageKey",
                StorageKeyName = "fake_StorageKeyName",
                KeyVaultUrl = @"https://localhost/keyvault",
            };
        }
    }
}
