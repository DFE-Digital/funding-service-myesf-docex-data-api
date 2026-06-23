using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The matching value filter.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    public interface IMatchingValueFilter<TElement> : IFilter<TElement>
    {
        /// <summary>
        /// Gets whether the element matches the given value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the element is inside the range / False otherwise.</returns>
        Task<bool> DoesElementMatchValue(TElement element, string value);
    }
}