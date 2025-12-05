using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CDR.Register.API.Logger
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddRequestResponseLogging(this IServiceCollection services)
            => services.AddSingleton<IRequestResponseLogger, RequestResponseLogger>();

        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
            => builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
