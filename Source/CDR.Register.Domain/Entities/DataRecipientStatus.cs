using System;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientStatusV1
    {
        public Guid DataRecipientId { get; set; }
        public string DataRecipientStatus { get; set; }
    }

    public class DataRecipientStatus
    {
        public Guid LegalEntityId { get; set; }
        public string Status { get; set; }
    }
}