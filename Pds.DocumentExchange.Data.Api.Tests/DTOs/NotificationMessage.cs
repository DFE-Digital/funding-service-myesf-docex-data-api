using Newtonsoft.Json;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Tests.DTOs
{
    public class NotificationMessage
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
        public IReadOnlyCollection<string> ToAddresses { get; set; }

        /// <summary>
        /// Gets or sets the email cc addresses.
        /// </summary>
        [JsonProperty("CCEmailAddress")]
        public IReadOnlyCollection<string> CopyAddresses { get; set; }

        /// <summary>
        /// Gets or sets the user names.
        /// </summary>
        [JsonProperty("UserNames")]
        public IReadOnlyCollection<string> Usernames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use the generic base email template or use a tailored email template.
        /// </summary>
        public bool UseStandardEmailTemplate { get; set; } = true;
    }
}