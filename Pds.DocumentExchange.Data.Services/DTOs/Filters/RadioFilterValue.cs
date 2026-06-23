namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Class representing a radio filter value.
    /// </summary>
    public class RadioFilterValue
    {
        /// <summary>
        /// Gets or sets the radio filter value title, for display usage.
        /// E.g. "Filter by team".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the radio filter value - a string can be used to filter items.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this filter value is selected.
        /// </summary>
        public bool Selected { get; set; }
    }
}