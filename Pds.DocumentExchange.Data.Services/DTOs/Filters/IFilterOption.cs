namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Interface representing a filter option for indicating a filter that has been applied.
    /// </summary>
    public interface IFilterOption
    {
        /// <summary>
        /// Gets or sets the filter type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the filter key, which identifies the item property that the filter represents.
        /// E.g. "ProductIdentifier".
        /// </summary>
        public string Key { get; set; }
    }
}