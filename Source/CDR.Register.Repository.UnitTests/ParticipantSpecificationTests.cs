using System;
using System.Linq;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Specifications;

namespace CDR.Register.Repository.UnitTests
{
    /// <summary>
    /// Test Participation Specifications.
    /// </summary>
    public class ParticipantSpecificationTests
    {
        private readonly Entities.Participation _banking = new() { IndustryId = Industry.BANKING };

        private readonly Entities.Participation _energy = new() { IndustryId = Industry.ENERGY };

        private readonly Entities.Participation _telco = new() { IndustryId = Industry.TELCO };

        private readonly Entities.Participation _nonBankLending = new() { IndustryId = Industry.NONBANKLENDING };

        private readonly IQueryable<Entities.Participation> _participations;

        public ParticipantSpecificationTests()
        {
            this._participations = new Entities.Participation[]
                            {
                                this._banking,
                                this._energy,
                                this._telco,
                                this._nonBankLending,
                            }.AsQueryable();
        }

        [Fact]
        public void ExcludeNblIndustrySpecification_AppliesSuccessfully()
        {
            var underTest = new ParticipationsSpecifications.ExcludeNblIndustry();

            var result = underTest.Apply(this._participations);

            Assert.NotEqual(this._participations.Count(), result.Count());
            Assert.DoesNotContain(this._nonBankLending, result);
        }

        [Fact]
        public void AllIndustriesSpecification_AppliesSuccessfully()
        {
            var underTest = new ParticipationsSpecifications.AllIndustries();

            var result = underTest.Apply(this._participations);

            Assert.Equal(this._participations.Count(), result.Count());
        }
    }
}
