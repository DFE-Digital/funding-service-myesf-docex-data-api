using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Factories
{
    /// <summary>
    /// I create notification message bodies (contract).
    /// </summary>
    public interface ICreateNotificationMessageBodies
    {
        /// <summary>
        /// Build the message content from...
        /// </summary>
        /// <param name="bodyTemplate">The body template.</param>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The completed body content.</returns>
        Task<string> BuildMessageContentFrom(string bodyTemplate, IBatchMetadataAnalysis batchAnalysis);

        /// <summary>
        /// Build the message content from...
        /// </summary>
        /// <param name="bodyTemplate">The body template.</param>
        /// <param name="productGroups">The product groups.</param>
        /// <returns>The completed body content.</returns>
        Task<string> BuildMessageContentFrom(string bodyTemplate, IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups);

        /// <summary>
        /// Build the personalisation dictionary for Notify templates from...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <param name="isForInfectedFiles">A value to determine whether the personalisation data is generated for an infected file or not.</param>
        /// <param name="recipientName">Recipient name to be added to email greeting.</param>
        /// <returns>The personalisation dictionary to replace the tokens in Notify with.</returns>
        Task<Dictionary<string, dynamic>> BuildEmailPersonalisationFrom(IBatchMetadataAnalysis batchAnalysis, bool isForInfectedFiles, string recipientName);

        /// <summary>
        /// Build the personalisation dictionary for Notify templates from...
        /// </summary>
        /// <param name="productGroups">The product groups.</param>
        /// <returns>The personalisation dictionary to replace the tokens in Notify with.</returns>
        Task<Dictionary<string, dynamic>> BuildEmailPersonalisationFrom(IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups);
    }
}