using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Represents the result from a filtering service.
    /// </summary>
    /// <typeparam name="TElement">The filtered element type.</typeparam>
    public class FilterResult<TElement>
    {
        /// <summary>
        /// Gets or sets the filtered items.
        /// </summary>
        public IEnumerable<TElement> Items { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        public IEnumerable<IFilter> Filters { get; set; }
    }
}