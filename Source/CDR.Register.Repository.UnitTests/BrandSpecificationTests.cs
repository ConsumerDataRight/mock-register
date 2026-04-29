using System;
using System.Linq;
using CDR.Register.Repository.Infrastructure;
using CDR.Register.Repository.Specifications;

namespace CDR.Register.Repository.UnitTests
{
    /// <summary>
    /// Test Brand Specifications.
    /// </summary>
    public class BrandSpecificationTests
    {
        private readonly Entities.Brand _banking =
            new() { BrandName = nameof(Industry.BANKING), Participation = new() { IndustryId = Industry.BANKING } };

        private readonly Entities.Brand _energy =
            new() { BrandName = nameof(Industry.ENERGY), Participation = new() { IndustryId = Industry.ENERGY } };

        private readonly Entities.Brand _telco =
            new() { BrandName = nameof(Industry.TELCO), Participation = new() { IndustryId = Industry.TELCO } };

        private readonly Entities.Brand _nonBankLending =
            new() { BrandName = nameof(Industry.NONBANKLENDING), Participation = new() { IndustryId = Industry.NONBANKLENDING } };

        private readonly IQueryable<Entities.Brand> _brands;

        public BrandSpecificationTests()
        {
            this._brands = new Entities.Brand[]
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
            var underTest = new BrandSpecifications.ExcludeNblIndustry();

            var result = underTest.Apply(this._brands);

            Assert.NotEqual(this._brands.Count(), result.Count());
            Assert.DoesNotContain(this._nonBankLending, result);
        }

        [Fact]
        public void AllIndustriesSpecification_AppliesSuccessfully()
        {
            var underTest = new BrandSpecifications.AllIndustries();

            var result = underTest.Apply(this._brands);

            Assert.Equal(this._brands.Count(), result.Count());
        }
    }
}
