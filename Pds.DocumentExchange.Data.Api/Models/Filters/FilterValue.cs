namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Class representing a filter value.
    /// </summary>
    public class FilterValue
    {
        /// <summary>
        /// Gets or sets the filter value title, for display usage.
        /// E.g. "Financial report".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the filter value - a string can be used to filter items
        /// by whether they have this value for the property specified in <see cref="IFilter.Key"/>.
        /// E.g. "10087".
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this filter value is selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the count of items that have this filter value.
        /// </summary>
        public int Count { get; set; }
    }
}