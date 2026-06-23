using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <inheritdoc cref="INotificationSummary"/>
    public sealed class NotificationSummary : INotificationSummary
    {
        /// <inheritdoc/>
        public NotificationType NotificationType { get; set; } = NotificationType.NotSet;

        /// <inheritdoc/>
        public IReadOnlyCollection<string> From { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> To { get; set; } = Array.Empty<string>();

        /// <inheritdoc/>
        public int Ukprn { get; set; }

        /// <inheritdoc/>
        public string Notes { get; set; }
    }
}