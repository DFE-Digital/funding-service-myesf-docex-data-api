using Pds.DocumentExchange.Data.Api.Models.Filters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Contains filter and pagination data for a list of items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    public class ListResult<T>
    {
        /// <summary>
        /// Gets or sets the list of items.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

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