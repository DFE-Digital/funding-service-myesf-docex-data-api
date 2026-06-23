using Pds.DocumentExchange.Data.Services.DTOs.Filters;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing methods to filter elements by applying multiple list filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface IListFilterService<TElement> : IFilterService<TElement, IListFilter<TElement>, ListFilterOption>
    {
    }
}