using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing a method to transform a FilterResult into a ListResult object.
    /// </summary>
    /// <typeparam name="TElement">The contained items type.</typeparam>
    public interface IFilterToListResultConverter<TElement>
    {
        /// <summary>
        /// Converts a FilterResult into a ListResult object.
        /// </summary>
        /// <param name="filterResult">The filter result.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>A ListResult object.</returns>
        ListResult<TElement> Convert(FilterResult<TElement> filterResult, int pageSize, int pageNumber);
    }
}