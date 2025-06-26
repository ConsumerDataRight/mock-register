// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("AuthDetail")]
    internal class AuthDetail
    {
        [ExplicitKey]
        public Guid BrandId { get; set; }

        public int? RegisterUTypeId { get; set; } = 1;

        public string? JwksEndpoint { get; set; } = "foo";
    }
}
