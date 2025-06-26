using Microsoft.Extensions.DependencyInjection;

namespace CDR.Register.API.Logger
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddRequestResponseLogging(this IServiceCollection services)
        {
            services.AddSingleton<IRequestResponseLogger, RequestResponseLogger>();
            return services;
        }
    }
}
