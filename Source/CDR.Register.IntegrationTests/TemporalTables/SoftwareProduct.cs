// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("SoftwareProduct")]
    internal class SoftwareProduct
    {
        [ExplicitKey]
        public Guid SoftwareProductId { get; set; } = Guid.NewGuid();

        public string? SoftwareProductName { get; set; } = "foo";

        public string? SoftwareProductDescription { get; set; } = "foo";

        public string? LogoUri { get; set; } = "foo";

        public string? SectorIdentifierUri { get; set; } = "foo";

        public string? ClientUri { get; set; } = "foo";

        public string? TosUri { get; set; } = "foo";

        public string? PolicyUri { get; set; } = "foo";

        public string? RecipientBaseUri { get; set; } = "foo";

        public string? RevocationUri { get; set; } = "foo";

        public string? RedirectUris { get; set; } = "foo";

        public string? JwksUri { get; set; } = "foo";

        public string? Scope { get; set; } = "foo";

        public int? StatusId { get; set; } = 1;

        public Guid? BrandId { get; set; }
    }
}
