using Pds.DocumentExchange.Data.Services.DTOs.Filters;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing methods to filter elements by applying multiple text-box filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public interface ITextBoxFilterService<TElement> : IFilterService<TElement, IMatchingTextBoxValueFilter<TElement>, TextBoxFilterOption>
    {
    }
}