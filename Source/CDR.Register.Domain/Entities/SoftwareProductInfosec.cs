using System.Collections.Generic;

namespace CDR.Register.Domain.Entities
{
    public class SoftwareProductInfosec
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string JwksUri { get; set; }
        public IEnumerable<SoftwareProductCertificateInfosec> X509Certificates { get; set; }
    }
}
