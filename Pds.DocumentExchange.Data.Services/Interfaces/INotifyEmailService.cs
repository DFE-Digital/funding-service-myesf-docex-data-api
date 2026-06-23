using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing a method to delete a collection of documents.
    /// </summary>
    public interface INotifyEmailService
    {
        /// <summary>
        /// Pushes a message on to the queue.
        /// </summary>
        /// <param name="message">The message to be pushed.</param>
        /// <returns>A collection of BatchMetadata objects.</returns>
        Task Push(INotificationMessage message);
    }
}