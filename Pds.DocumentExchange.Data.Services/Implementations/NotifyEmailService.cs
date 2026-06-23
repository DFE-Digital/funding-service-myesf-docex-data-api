using Pds.Core.Notification.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotifyNotificationModels = Pds.Core.Notification.Models;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The batches service.
    /// </summary>
    public class NotifyEmailService : INotifyEmailService
    {
        private readonly INotificationEmailQueueService _notificationEmailQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyEmailService"/> class.
        /// </summary>
        /// <param name="notificationEmailQueueService">The notification Email Queue Service.</param>
        public NotifyEmailService(
           INotificationEmailQueueService notificationEmailQueueService)
        {
            _notificationEmailQueueService = notificationEmailQueueService;
        }

        /// <inheritdoc/>
        public async Task Push(INotificationMessage message)
        {
            NotifyNotificationModels.NotificationMessage notifyMessage = new NotifyNotificationModels.NotificationMessage();

            notifyMessage.EmailAddresses = message.ToAddresses;
            notifyMessage.EmailMessageType = message.EmailMessageType;
            notifyMessage.RequestingService = "DocEx";
            notifyMessage.EmailPersonalisation = new NotifyNotificationModels.GovUkNotifyPersonalisation();
            notifyMessage.EmailPersonalisation.Personalisation = new Dictionary<string, object>();

            foreach (var item in message.EmailPersonalisation)
            {
                notifyMessage.EmailPersonalisation.Personalisation.Add(item.Key.Replace("[", string.Empty).Replace("]", string.Empty), item.Value);
            }

            await _notificationEmailQueueService.SendAsync(notifyMessage);
        }
    }
}