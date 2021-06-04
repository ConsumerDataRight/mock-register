using System;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientStatus
    {
        public Guid DataRecipientId { get; set; }
        public string Status { get; set; }
    }
}
