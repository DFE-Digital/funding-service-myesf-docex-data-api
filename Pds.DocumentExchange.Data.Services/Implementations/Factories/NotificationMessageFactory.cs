using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Factories
{
    /// <summary>
    /// the document exchange notification message factory.
    /// </summary>
    public sealed class NotificationMessageFactory :
        ICreateNotificationMessages
    {
        /// <inheritdoc/>
        public async Task<INotificationMessage> Create(
            INotificationMessageHeader messageHeader,
            string messageBody,
            bool useStandardTemplate,
            string parentBatchId,
            int ukprn,
            string emailMessageType = null,
            Dictionary<string, dynamic> emailPersonalisation = null) =>
            await Task.FromResult(
                new NotificationMessage
                {
                    FromAddress = messageHeader.FromAddress,
                    ToAddresses = messageHeader.ToAddresses,
                    CopyAddresses = messageHeader.CopyAddresses,
                    Subject = messageHeader.Subject,
                    Content = messageBody,
                    Usernames = messageHeader.Usernames,
                    UseStandardEmailTemplate = useStandardTemplate,
                    ParentBatchId = parentBatchId,
                    Ukprn = ukprn.ToString(),
                    EmailMessageType = emailMessageType,
                    EmailPersonalisation = emailPersonalisation
                });
    }
}