// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("ParticipationStatus")]
    internal class ParticipationStatus
    {
        [ExplicitKey]
        public int? ParticipationStatusId { get; set; }

        public string? ParticipationStatusCode { get; set; }
    }
}
