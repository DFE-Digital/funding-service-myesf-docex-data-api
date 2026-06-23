using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
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
        public string Type
            => FilterType.RadioFilter.ToString();

        /// <summary>
        /// Gets or sets the radio filter values, which are the data values available to be applied for this filter.
        /// </summary>
        public IEnumerable<RadioFilterValue> Values { get; set; }
    }
}