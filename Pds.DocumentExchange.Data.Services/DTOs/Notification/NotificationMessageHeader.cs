using Pds.Services.Common.Helpers;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// Notification message header (implementation).
    /// </summary>
    internal sealed class NotificationMessageHeader :
        INotificationMessageHeader
    {
        /// <inheritdoc/>
        public string Subject { get; set; }

        /// <inheritdoc/>
        public string FromAddress { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<string> ToAddresses { get; set; } = Collection.EmptyAndReadOnly<string>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> CopyAddresses { get; set; } = Collection.EmptyAndReadOnly<string>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Usernames { get; set; } = Collection.EmptyAndReadOnly<string>();
    }
}