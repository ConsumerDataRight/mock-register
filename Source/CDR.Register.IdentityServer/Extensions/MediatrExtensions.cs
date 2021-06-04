using System.Threading.Tasks;
using CDR.Register.IdentityServer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CDR.Register.IdentityServer.Extensions
{
    public static class MediatrExtensions
    {
        public static async Task LogErrorAndPublish<T>(this IMediator mediator, NotificationMessage notificationMessage, ILogger<T> logger)
        {
            logger.LogError($"Error: {notificationMessage.Code}: {notificationMessage.Content}");
            await mediator.Publish(notificationMessage);
        }
    }
}
