using MediatR;

namespace CDR.Register.API.Infrastructure.Models
{
    public class NotificationMessage<T> : INotification
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public T Content { get; set; }
    }

    // Default notification message string content
    public class NotificationMessage : NotificationMessage<string>
    {
        public NotificationMessage(string type, string code, string description, string content = null)
        {
            Type = type;
            Code = code;
            Description = description;
            Content = content;
        }
    }
}
