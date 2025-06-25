// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("LegalEntity")]
    internal class LegalEntity
    {
        [ExplicitKey]
        public Guid LegalEntityId { get; set; } = Guid.NewGuid();

        public string? LegalEntityName { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";
    }
}
