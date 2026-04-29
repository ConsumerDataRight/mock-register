using System.Linq;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.Repository.Specifications
{
    /// <summary>
    /// Specifications for filtering <see cref="Brand"/> records.
    /// </summary>
    public class BrandSpecifications
    {
        /// <summary>
        /// Allow <see cref="Brand"/> records with a <see cref="Participation"/> for any industry.
        /// </summary>
        public class AllIndustries : IBrandSpecification
        {
            /// <inheritdoc />
            public IQueryable<Brand> Apply(IQueryable<Brand> items)
            {
                return items;
            }
        }

        /// <summary>
        /// Only allow <see cref="Brand"/> records with a <see cref="Participation"/> that is not <see cref="Industry.NONBANKLENDING"/>.
        /// </summary>
        public class ExcludeNblIndustry : IBrandSpecification
        {
            /// <inheritdoc />
            public IQueryable<Brand> Apply(IQueryable<Brand> items)
            {
                return items.Where(i => i.Participation.IndustryId != Industry.NONBANKLENDING);
            }
        }
    }
}
