using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Class representing a list filter option.
    /// </summary>
    public class ListFilterOption : IFilterOption
    {
        /// <inheritdoc/>
        public string Type { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the list of values that have been applied for this filter.
        /// E.g. ["10001", "10087", ...].
        /// </summary>
        public IEnumerable<string> Values { get; set; }
    }
}