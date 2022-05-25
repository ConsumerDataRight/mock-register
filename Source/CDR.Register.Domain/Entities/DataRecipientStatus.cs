using System;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientStatus
    {
        public Guid DataRecipientId { get; set; }
        public string Status { get; set; }
    }

    public class DataRecipientStatusV2
    {
        public Guid LegalEntityId { get; set; }
        public string Status { get; set; }
    }
}