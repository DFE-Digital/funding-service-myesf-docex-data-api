using System;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Class representing an email setting.
    /// </summary>
    public class EmailSetting
    {
        /// <summary>
        /// Gets or sets the email message type.
        /// </summary>
        public string EmailMessageType { get; set; }

        /// <summary>
        /// Gets or sets the date and time the product was last updated.
        /// </summary>
        /// <remarks>Null if the date and time are unknown.</remarks>
        public DateTime? LastUpdated { get; set; }
    }
}