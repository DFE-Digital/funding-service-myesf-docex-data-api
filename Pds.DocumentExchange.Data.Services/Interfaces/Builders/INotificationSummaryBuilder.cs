using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Builders
{
    /// <summary>
    /// Builds notification summaries.
    /// </summary>
    public interface INotificationSummaryBuilder
    {
        /// <summary>
        /// Builds a notification summary.
        /// </summary>
        /// <param name="notificationType">The notification type.</param>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="recipient">The recipient address.</param>
        /// <returns>The summary.</returns>
        Task<INotificationSummary> BuildSummary(NotificationType notificationType, int ukprn, string notes, string recipient);

        /// <summary>
        /// Builds a notification summary.
        /// </summary>
        /// <param name="notificationType">The notification type.</param>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="notes">The notes.</param>
        /// <param name="recipients">The recipient addresses.</param>
        /// <returns>The summary.</returns>
        Task<INotificationSummary> BuildSummary(NotificationType notificationType, int ukprn, string notes, IReadOnlyCollection<string> recipients);

        /// <summary>
        /// Saves a batch notification summary.
        /// </summary>
        /// <param name="batchNotificationSummary">The batch notification summary.</param>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The currently running task.</returns>
        Task SaveSummary(BatchNotificationSummary batchNotificationSummary, IBatchMetadataAnalysis batchAnalysis);
    }
}