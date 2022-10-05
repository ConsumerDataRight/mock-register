namespace CDR.Register.API.Logger
{
    using Microsoft.Extensions.DependencyInjection;

    public static class LoggerExtensions
    {
        public static IServiceCollection AddRequestResponseLogging(this IServiceCollection services)
        {
            services.AddSingleton<IRequestResponseLogger, RequestResponseLogger>();
            return services;
        }
    }
}