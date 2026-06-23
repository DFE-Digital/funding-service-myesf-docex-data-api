namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Interface representing a single value filter option for indicating a filter that has been applied.
    /// </summary>
    public interface ISingleValueFilterOption : IFilterOption
    {
        /// <summary>
        /// Gets or sets the value that has been applied for this filter.
        /// </summary>
        public string Value { get; set; }
    }
}