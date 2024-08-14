using CDR.Register.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CDR.Register.Domain.Extensions
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddCdrNewtonsoftJson(this IMvcBuilder mvcBuilder)
        {
            var defaultSettings = new CdrJsonSerializerSettings();
            JsonConvert.DefaultSettings = () => defaultSettings;

            return mvcBuilder.AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = defaultSettings.ContractResolver;
                options.SerializerSettings.DefaultValueHandling = defaultSettings.DefaultValueHandling;
                options.SerializerSettings.NullValueHandling = defaultSettings.NullValueHandling;
                options.SerializerSettings.Formatting = defaultSettings.Formatting;
                options.SerializerSettings.Converters = defaultSettings.Converters;
            }
            );
        }
    }
}
