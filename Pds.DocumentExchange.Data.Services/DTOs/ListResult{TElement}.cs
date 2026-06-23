using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Contains filter and pagination data for a list of items.
    /// </summary>
    /// <typeparam name="TElement">The type of the items in the list.</typeparam>
    public class ListResult<TElement>
    {
        /// <summary>
        /// Gets or sets the list of items.
        /// </summary>
        public IEnumerable<TElement> Items { get; set; }

        /// <summary>
        /// Gets or sets the total number of items.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the filters that are available.
        /// </summary>
        public IEnumerable<IFilter> Filters { get; set; }
    }
}