using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Class representing a filter group.
    /// </summary>
    public class FilterGroup
    {
        /// <summary>
        /// Gets or sets the filter group title, for display usage.
        /// E.g. "Financial report".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the filter values, which are the data values available to be applied for this filter.
        /// E.g. { "Financial report", 10087, ... }.
        /// </summary>
        public IEnumerable<FilterValue> Values { get; set; }
    }
}