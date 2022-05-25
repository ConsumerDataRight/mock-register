using System;

namespace CDR.Register.Domain.Entities
{
    public class DataHolderStatus
    {
        public Guid LegalEntityId { get; set; }
        public string Status { get; set; }
    }
}
