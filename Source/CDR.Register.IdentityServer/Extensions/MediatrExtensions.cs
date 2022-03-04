using CDR.Register.IdentityServer.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Threading.Tasks;

namespace CDR.Register.IdentityServer.Extensions
{
    public static class MediatrExtensions
    {
        public static async Task LogErrorAndPublish<T>(this IMediator mediator, NotificationMessage notificationMessage, ILogger<T> logger)
        {
            using (LogContext.PushProperty("MethodName", "LogErrorAndPublish"))
            {
                logger.LogError($"Error: {notificationMessage.Code}: {notificationMessage.Content}");
            }
            await mediator.Publish(notificationMessage);
        }
    }
}