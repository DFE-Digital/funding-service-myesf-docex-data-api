using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Builders
{
    /// <summary>
    /// Notification message builder.
    /// </summary>
    public interface INotificationMessageBuilder
    {
        /// <summary>
        /// Get infect file message.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="batchAnalysis">The batch containing the information.</param>
        /// <param name="recipientEmail">The recipient email.</param>
        /// <param name="userName">The recipient name.</param>
        /// <returns>A notification message.</returns>
        Task<INotificationMessage> GetInfectedFileMessage(string subject, IBatchMetadataAnalysis batchAnalysis, string recipientEmail, string userName);

        /// <summary>
        /// Get clean file message.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="batchAnalysis">The batch containing the information.</param>
        /// <param name="recipientEmail">The recipient email.</param>
        /// <param name="userName">The recipient name.</param>
        /// <returns>A notification message.</returns>
        Task<INotificationMessage> GetCleanFileMessage(string subject, IBatchMetadataAnalysis batchAnalysis, string recipientEmail, string userName);

        /// <summary>
        /// Get agency team message.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="batchAnalysis">The batch containing the information.</param>
        /// <param name="teamEmail">The recipient email.</param>
        /// <param name="teamName">The recipient name.</param>
        /// <returns>A notification message.</returns>
        Task<INotificationMessage> GetAgencyTeamMessage(string subject, IBatchMetadataAnalysis batchAnalysis, string teamEmail, string teamName);

        /// <summary>
        /// Get document user message.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="productGroups">The product group (changes).</param>
        /// <param name="recipientEmails">The recipient email addresses.</param>
        /// <param name="emailNames">The recipient email names.</param>
        /// <param name="parentBatchId">The parent batch identifier.</param>
        /// <param name="ukprn">The Ukprn.</param>
        /// <returns>A notification message.</returns>
        Task<INotificationMessage> GetDocumentUserMessage(string subject, IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups, IReadOnlyCollection<string> recipientEmails, IReadOnlyCollection<string> emailNames, string parentBatchId, int ukprn);
    }
}