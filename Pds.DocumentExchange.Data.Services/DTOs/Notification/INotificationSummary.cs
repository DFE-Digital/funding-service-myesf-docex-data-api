using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// Notification summary data.
    /// </summary>
    public interface INotificationSummary
    {
        /// <summary>
        /// Gets the type of notification.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        NotificationType NotificationType { get; }

        /// <summary>
        /// Gets the from (address list).
        /// </summary>
        IReadOnlyCollection<string> From { get; }

        /// <summary>
        /// Gets the to (address list).
        /// </summary>
        IReadOnlyCollection<string> To { get; }

        /// <summary>
        /// Gets the UKPRN of the relevant organisation.
        /// </summary>
        int Ukprn { get; }

        /// <summary>
        /// Gets any other notes to log.
        /// </summary>
        string Notes { get; }
    }
}