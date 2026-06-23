using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Class representing a radio button filter that can be applied against some items.
    /// </summary>
    public class RadioFilter : IFilter
    {
        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the radio filter values, which are the data values available to be applied for this filter.
        /// </summary>
        public IEnumerable<RadioFilterValue> Values { get; set; }
    }
}