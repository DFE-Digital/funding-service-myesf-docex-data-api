using System;

namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Class representing a date filter option for indicating a date range filter that has been applied.
    /// </summary>
    public class DateRangeFilterOption : IFilterOption
    {
        /// <inheritdoc/>
        public string Type { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the 'from' date value.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Gets or sets the 'to' date value.
        /// </summary>
        public DateTime? To { get; set; }
    }
}