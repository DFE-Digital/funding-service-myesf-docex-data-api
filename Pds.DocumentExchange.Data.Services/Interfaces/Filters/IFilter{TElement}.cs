using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing properties to define a filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface IFilter<TElement>
    {
        /// <summary>
        /// Gets the filter title.
        /// </summary>
        string FilterTitle { get; }

        /// <summary>
        /// Gets the filter key.
        /// </summary>
        string FilterKey { get; }

        /// <summary>
        /// Gets the filter type.
        /// </summary>
        FilterType FilterType { get; }
    }
}