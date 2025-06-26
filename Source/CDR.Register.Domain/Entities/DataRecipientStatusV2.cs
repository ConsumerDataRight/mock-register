using System;

namespace CDR.Register.Domain.Entities
{
    public class DataRecipientStatusV2
    {
        public Guid LegalEntityId { get; set; }

        public string Status { get; set; }
    }
}
