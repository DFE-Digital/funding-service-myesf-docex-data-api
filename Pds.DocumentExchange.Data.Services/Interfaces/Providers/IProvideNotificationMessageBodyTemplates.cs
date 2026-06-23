using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Providers
{
    /// <summary>
    /// I provide notification message body templates (contract).
    /// </summary>
    public interface IProvideNotificationMessageBodyTemplates
    {
        /// <summary>
        /// Get the clean file template for...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The document template.</returns>
        Task<string> GetCleanFileTemplateFor(IBatchMetadataAnalysis batchAnalysis);

        /// <summary>
        /// Get the infected file template for...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The document template.</returns>
        Task<string> GetInfectedFileTemplateFor(IBatchMetadataAnalysis batchAnalysis);

        /// <summary>
        /// Get the team notification template for...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The document template.</returns>
        Task<string> GetTeamNotificationTemplateFor(IBatchMetadataAnalysis batchAnalysis);

        /// <summary>
        /// Get the document user message template for...
        /// </summary>
        /// <param name="productGroups">The product groups.</param>
        /// <returns>The document template.</returns>
        Task<string> GetDocumentUserMessageTemplateFor(IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups);
    }
}