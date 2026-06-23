using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing properties and methods to define a list filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface IListFilter<TElement> : IFilter<TElement>
    {
        /// <summary>
        /// Gets the function that tells how to get the filter value from an element.
        /// </summary>
        Func<TElement, Task<string>> GetFilterValueFromElement { get; }

        /// <summary>
        /// Gets the function that tells how to get the filter title from the filter value.
        /// </summary>
        Func<string, Task<string>> GetFilterTitleFromValue { get; }

        /// <summary>
        /// Gets the function that tells how to get the filter category from the filter value.
        /// </summary>
        Func<string, Task<string>> GetFilterCategoryFromValue { get; }

        /// <summary>
        /// Returns the elements corresponding to the specified filter value.
        /// </summary>
        /// <param name="value">The filter value.</param>
        /// <returns>A read-only collection containing the elements that match the filter value.</returns>
        Task<IReadOnlyCollection<TElement>> GetElementsByFilterValue(string value);

        /// <summary>
        /// Returns the elements corresponding to all the specified filter values.
        /// </summary>
        /// <param name="values">The filter values.</param>
        /// <returns>A read-only collection containing the elements that match all the filter value.</returns>
        Task<IReadOnlyCollection<TElement>> GetElementsByFilterValue(IEnumerable<string> values);

        /// <summary>
        /// Gets the values the filter can filter by.
        /// </summary>
        /// <returns>A read-only collection containing all the filter values.</returns>
        Task<IReadOnlyCollection<string>> GetFilterValues();

        /// <summary>
        /// Gets the filter type.
        /// </summary>
        ListFilterType ListFilterType { get; }
    }
}