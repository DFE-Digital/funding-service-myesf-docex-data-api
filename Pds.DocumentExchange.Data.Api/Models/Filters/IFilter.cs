namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Interface representing the data for a filter that can be applied against some items.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Gets or sets the filter title, for display usage.
        /// E.g. "Document Type".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the filter key, which identifies the item property that the filter represents.
        /// E.g. "ProductIdentifier".
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the filter type.
        /// </summary>
        public string Type { get; set; }
    }
}