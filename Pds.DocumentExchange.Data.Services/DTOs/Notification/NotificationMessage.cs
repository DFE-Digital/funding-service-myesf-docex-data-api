using Newtonsoft.Json;
using Pds.Services.Common.Helpers;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// Notification message (implementation).
    /// </summary>
    internal sealed class NotificationMessage :
        INotificationMessage
    {
        /// <summary>
        /// Gets or sets the email subject line.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email body content.
        /// </summary>
        [JsonProperty("Body")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email sender address.
        /// </summary>
        [JsonProperty("FromEmailAddress")]
        public string FromAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email recipient addresses.
        /// </summary>
        [JsonProperty("ToEmailAddress")]
        public IReadOnlyCollection<string> ToAddresses { get; set; } = Collection.EmptyAndReadOnly<string>();

        /// <summary>
        /// Gets or sets the email cc addresses.
        /// </summary>
        [JsonProperty("CCEmailAddress")]
        public IReadOnlyCollection<string> CopyAddresses { get; set; } = Collection.EmptyAndReadOnly<string>();

        /// <summary>
        /// Gets or sets the user names.
        /// </summary>
        [JsonProperty("UserNames")]
        public IReadOnlyCollection<string> Usernames { get; set; } = Collection.EmptyAndReadOnly<string>();

        /// <summary>
        /// Gets or sets a value indicating whether use the generic base email template or use a tailored email template.
        /// </summary>
        public bool UseStandardEmailTemplate { get; set; } = true;

        /// <summary>
        /// Gets or sets the parent batch identifier.
        /// </summary>
        public string ParentBatchId { get; set; }

        /// <summary>
        /// Gets or sets the Ukprn.
        /// </summary>
        public string Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the message type which will be mapped to Notify email templates.
        /// </summary>
        public string EmailMessageType { get; set; }

        /// <summary>
        /// Gets or sets the tokens to be replaced in Notify email templates.
        /// </summary>
        public Dictionary<string, dynamic> EmailPersonalisation { get; set; }
    }
}