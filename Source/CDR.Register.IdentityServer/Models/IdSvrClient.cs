using System.Collections.Generic;

namespace CDR.Register.IdentityServer.Models
{
    public class IdSvrClient
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string JwksUri { get; set; }
        public List<IdSvrClientCertificate> X509Certificates { get; set; }

    }

    public class IdSvrClientCertificate
    {
        public string CommonName { get; set; }
        public string Thumbprint { get; set; }
    }

}
