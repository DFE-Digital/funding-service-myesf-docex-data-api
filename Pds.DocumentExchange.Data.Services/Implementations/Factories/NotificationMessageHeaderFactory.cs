using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Factories
{
    /// <summary>
    /// The notification message header factory (implementation).
    /// </summary>
    public sealed class NotificationMessageHeaderFactory :
        ICreateNotificationMessageHeaders
    {
        /// <inheritdoc/>
        public async Task<INotificationMessageHeader> Create(string subject, string from, string to, string name) =>
            await Create(subject, from, new[] { to }, new[] { name });

        /// <inheritdoc/>
        public async Task<INotificationMessageHeader> Create(string subject, string from, IReadOnlyCollection<string> to, IReadOnlyCollection<string> names) =>
            await Task.FromResult(
                new NotificationMessageHeader
                {
                    Subject = subject,
                    FromAddress = from,
                    ToAddresses = to,
                    Usernames = names
                });
    }
}