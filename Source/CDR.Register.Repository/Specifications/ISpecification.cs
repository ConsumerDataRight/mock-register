using System.Linq;

namespace CDR.Register.Repository.Specifications
{
    /// <summary>
    /// Represents a business rule or filtering criterion that can be applied to query.
    /// </summary>
    /// <typeparam name="T">The type of item being queried.</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Applies the specification's filtering or transformation logic to the items.
        /// </summary>
        /// <param name="items">The unmodified query.</param>
        /// <returns>
        /// The modified query with the rule/filtering applied.
        /// </returns>
        IQueryable<T> Apply(IQueryable<T> items);
    }
}
