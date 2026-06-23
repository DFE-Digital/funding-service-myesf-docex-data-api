using Pds.DocumentExchange.Data.Services.DTOs.Filters;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing methods to filter elements by applying multiple date range filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface IDateRangeFilterService<TElement> : IFilterService<TElement, IDateRangeFilter<TElement>, DateRangeFilterOption>
    {
    }
}