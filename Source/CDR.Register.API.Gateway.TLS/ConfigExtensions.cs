using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CDR.Register.API.Gateway.TLS
{
    public static class ConfigExtensions
    {
        public static IConfiguration BuildRegisterConfiguration(this ConfigurationBuilder builder, string[] args)
        {
            var configurationCommandLine = new ConfigurationBuilder()
                            .AddCommandLine(args).Build();

            var configuration = new ConfigurationBuilder()
                            .AddCommandLine(args)
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? configurationCommandLine.GetValue<string>("environment")}.json", true)
                            .Build();

            return configuration;
        }
    }
}
