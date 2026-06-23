using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Factories
{
    /// <summary>
    /// Performs analysis on document batch metadata.
    /// </summary>
    public interface IBatchMetadataAnalysisFactory
    {
        /// <summary>
        /// Analyses the batch with the given parent ID.
        /// </summary>
        /// <param name="parentId">The parent batch id.</param>
        /// <returns>The batch analysis.</returns>
        Task<IBatchMetadataAnalysis> AnalyseBatch(string parentId);

        /// <summary>
        /// Creates the batch analysis.
        /// </summary>
        /// <param name="batches">The collection of batches.</param>
        /// <param name="parentId">The parent batch id.</param>
        /// <returns>The batch analysis.</returns>
        Task<IBatchMetadataAnalysis> Create(IReadOnlyCollection<BatchMetadata> batches, string parentId);
    }
}