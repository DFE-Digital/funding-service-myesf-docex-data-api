using Newtonsoft.Json;
using Pds.DocumentExchange.Data.Api.JsonConverters;

namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Interface representing a filter option for indicating a filter that has been applied.
    /// </summary>
    [JsonConverter(typeof(FilterOptionJsonConverter))]
    public interface IFilterOption
    {
        /// <summary>
        /// Gets or sets the filter type.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the filter key, which identifies the item property that the filter represents.
        /// E.g. "ProductIdentifier".
        /// </summary>
        string Key { get; set; }
    }
}