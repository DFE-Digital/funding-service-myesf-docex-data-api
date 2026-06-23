using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The filters factory interface.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface IFiltersFactory<TElement>
    {
        /// <summary>
        /// Gets a collection of filters based on the specified keys.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        /// <param name="filterKeys">The filter keys.</param>
        /// <returns>The filters collection.</returns>
        IEnumerable<IFilter<TElement>> GetFilters(
            IEnumerable<TElement> elements,
            IEnumerable<FilterKey> filterKeys);
    }
}