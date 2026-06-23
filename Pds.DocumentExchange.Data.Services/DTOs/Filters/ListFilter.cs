using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Class representing the data for a list filter that can be applied against some items.
    /// </summary>
    public class ListFilter : IFilter
    {
        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Type
            => FilterType.ListFilter.ToString();

        /// <summary>
        /// Gets or sets the filter values, which are the data values available to be applied for this filter.
        /// E.g. { "Financial report", 10087, ... }.
        /// </summary>
        public IEnumerable<FilterValue> Values { get; set; }

        /// <summary>
        /// Gets or sets the filter groups.
        /// </summary>
        public IEnumerable<FilterGroup> Groups { get; set; }
    }
}