using System;

namespace CDR.Register.Domain.Entities
{
    public class SoftwareProductStatus
    {
        public Guid SoftwareProductId { get; set; }
        public string Status { get; set; }
    }
}
