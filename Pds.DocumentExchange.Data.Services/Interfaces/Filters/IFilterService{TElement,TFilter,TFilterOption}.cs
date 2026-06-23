using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing methods to filter elements by applying multiple filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    /// <typeparam name="TFilter">The filter type to be used.</typeparam>
    /// <typeparam name="TFilterOption">The filter option type defining the selected filter values to be applied.</typeparam>
    public interface IFilterService<TElement, TFilter, TFilterOption>
        where TFilterOption : IFilterOption
    {
        /// <summary>
        /// Filters the elements collection by the specified filters and updates the applicable filters.
        /// </summary>
        /// <param name="elements">The elements collection.</param>
        /// <param name="filters">The collection of filters.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <returns>The filter result containing the filtered collection of elements and
        /// the updated collection of filter values.</returns>
        Task<FilterResult<TElement>> FilterAndGetUpdatedFilters(
            IEnumerable<TElement> elements,
            IEnumerable<TFilter> filters,
            IEnumerable<TFilterOption> filterOptions);
    }
}