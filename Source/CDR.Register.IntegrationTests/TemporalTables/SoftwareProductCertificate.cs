// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("SoftwareProductCertificate")]
    internal class SoftwareProductCertificate
    {
        [ExplicitKey]
        public Guid SoftwareProductCertificateId { get; set; } = Guid.NewGuid();

        public Guid SoftwareProductId { get; set; }

        public string? CommonName { get; set; } = "foo";

        public string? Thumbprint { get; set; } = "foo";
    }
}
