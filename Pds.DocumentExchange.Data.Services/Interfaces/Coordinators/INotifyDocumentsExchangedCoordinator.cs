using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Coordinators
{
    /// <summary>
    /// Coordinates notifications for exchanged documents.
    /// </summary>
    public interface INotifyDocumentsExchangedCoordinator
    {
        /// <summary>
        /// Notifies the users.
        /// </summary>
        /// <param name="parentBatchId">The parent batch id.</param>
        /// <returns>A task returning the batch notification summary.</returns>
        Task<BatchNotificationSummary> NotifyUsers(string parentBatchId);
    }
}
