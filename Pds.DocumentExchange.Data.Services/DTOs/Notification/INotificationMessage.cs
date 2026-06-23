using Newtonsoft.Json;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// I notification message (contract).
    /// </summary>
    public interface INotificationMessage
    {
        /// <summary>
        /// Gets the email subject line.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Gets the email body content.
        /// </summary>
        [JsonProperty("Body")]
        string Content { get; }

        /// <summary>
        /// Gets the email sender address.
        /// </summary>
        [JsonProperty("FromEmailAddress")]
        string FromAddress { get; }

        /// <summary>
        /// Gets the email recipient addresses.
        /// </summary>
        [JsonProperty("ToEmailAddress")]
        IReadOnlyCollection<string> ToAddresses { get; }

        /// <summary>
        /// Gets the email cc addresses.
        /// </summary>
        [JsonProperty("CCEmailAddress")]
        IReadOnlyCollection<string> CopyAddresses { get; }

        /// <summary>
        /// Gets the user names.
        /// </summary>
        [JsonProperty("UserNames")]
        IReadOnlyCollection<string> Usernames { get; }

        /// <summary>
        /// Gets a value indicating whether use the generic base email template or use a tailored email template.
        /// </summary>
        bool UseStandardEmailTemplate { get; }

        /// <summary>
        /// Gets the parent batch identifier.
        /// </summary>
        [JsonProperty("ParentBatchId")]
        string ParentBatchId { get; }

        /// <summary>
        /// Gets the Ukprn.
        /// </summary>
        [JsonProperty("Ukprn")]
        string Ukprn { get; }

        /// <summary>
        /// Gets the message type which will be mapped to Notify email templates.
        /// </summary>
        string EmailMessageType { get; }

        /// <summary>
        /// Gets the tokens to be replaced in Notify email templates.
        /// </summary>
        Dictionary<string, dynamic> EmailPersonalisation { get; }
    }
}