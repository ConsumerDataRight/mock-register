using System.Linq;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;

namespace CDR.Register.Repository.Specifications
{
    /// <summary>
    /// Specifications for filtering <see cref="Participation"/> records.
    /// </summary>
    public class ParticipationsSpecifications
    {
        /// <summary>
        /// Allow <see cref="Participation"/> records for any industry.
        /// </summary>
        public class AllIndustries : IParticipationSpecification
        {
            /// <inheritdoc />
            public IQueryable<Participation> Apply(IQueryable<Participation> items)
            {
                return items;
            }
        }

        /// <summary>
        /// Only allow <see cref="Participation"/> records that are not <see cref="Industry.NONBANKLENDING"/>.
        /// </summary>
        public class ExcludeNblIndustry : IParticipationSpecification
        {
            /// <inheritdoc />
            public IQueryable<Participation> Apply(IQueryable<Participation> items)
            {
                return items.Where(i => i.IndustryId != Infrastructure.Industry.NONBANKLENDING);
            }
        }
    }
}
