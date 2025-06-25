// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("Brand")]
    internal class Brand
    {
        [ExplicitKey]
        public Guid BrandId { get; set; } = Guid.NewGuid();

        public string? BrandName { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";

        public int? BrandStatusId { get; set; } = 1;

        public Guid? ParticipationId { get; set; }

        public DateTime? LastUpdated { get; set; } = DateTime.Now;
    }
}
