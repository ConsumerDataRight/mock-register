// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("Endpoint")]
    internal class Endpoint
    {
        [ExplicitKey]
        public Guid BrandId { get; set; }

        public string? Version { get; set; } = "foo";

        public string? PublicBaseUri { get; set; } = "foo";

        public string? ResourceBaseUri { get; set; } = "foo";

        public string? InfosecBaseUri { get; set; } = "foo";

        public string? ExtensionBaseUri { get; set; } = "foo";

        public string? WebsiteUri { get; set; } = "foo";
    }
}
