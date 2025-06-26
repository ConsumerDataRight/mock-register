// #define DEBUG_WRITE_EXPECTED_AND_ACTUAL_JSON

using System;
using Dapper.Contrib.Extensions;

#nullable enable

namespace CDR.Register.IntegrationTests.TemporalTables
{
    [Table("Participation")]
    internal class Participation
    {
        [ExplicitKey]
        public Guid ParticipationId { get; set; } = Guid.NewGuid();

        public Guid? LegalEntityId { get; set; }

        public int? ParticipationTypeId { get; set; } = 1;

        public int? IndustryId { get; set; } = 1;

        public int? StatusId { get; set; } = 1;
    }
}
