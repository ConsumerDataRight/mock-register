using MediatR;

namespace CDR.Register.IdentityServer.Models
{
    public class NotificationMessage : INotification
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }

        public NotificationMessage(string type, string code, string description, string content = null)
        {
            Type = type;
            Code = code;
            Description = description;
            Content = content;
        }
    }
}
