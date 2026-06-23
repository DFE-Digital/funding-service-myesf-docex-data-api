using Pds.DocumentExchange.Data.Api.Models.Filters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Contains options for filtering and paginating a list of items.
    /// </summary>
    public class ListOptions
    {
        /// <summary>
        /// Gets or sets the filters to apply.
        /// </summary>
        public IEnumerable<IFilterOption> FilterOptions { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; } = 1;
    }
}