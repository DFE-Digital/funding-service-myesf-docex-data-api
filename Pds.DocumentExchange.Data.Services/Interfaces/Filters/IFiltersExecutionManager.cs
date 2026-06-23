using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// Interface providing a method to filter a list of documents based on the specified filter keys and
    /// selected options.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    public interface IFiltersExecutionManager<TDocument>
    {
        /// <summary>
        /// Filters a list of documents.
        /// </summary>
        /// <param name="documents">The document list.</param>
        /// <param name="filterOptions">The selected filter options.</param>
        /// <param name="filterKeys">The filter keys.</param>
        /// <returns>The filter result containing the filtered document list and the updated filters.</returns>
        Task<FilterResult<TDocument>> Filter(
            IEnumerable<TDocument> documents,
            IEnumerable<IFilterOption> filterOptions,
            IEnumerable<FilterKey> filterKeys);
    }
}