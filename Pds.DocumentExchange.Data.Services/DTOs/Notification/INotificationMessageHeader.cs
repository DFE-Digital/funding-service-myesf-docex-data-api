using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// I notification message header (contract).
    /// </summary>
    public interface INotificationMessageHeader
    {
        /// <summary>
        /// Gets the message subject line.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Gets the email sender address.
        /// </summary>
        string FromAddress { get; }

        /// <summary>
        /// Gets the message recipient addresses.
        /// </summary>
        IReadOnlyCollection<string> ToAddresses { get; }

        /// <summary>
        /// Gets the message copy addresses.
        /// </summary>
        IReadOnlyCollection<string> CopyAddresses { get; }

        /// <summary>
        /// Gets the user names.
        /// </summary>
        IReadOnlyCollection<string> Usernames { get; }
    }
}