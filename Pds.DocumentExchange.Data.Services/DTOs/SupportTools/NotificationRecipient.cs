namespace Pds.DocumentExchange.Data.Services.DTOs.SupportTools
{
    /// <summary>
    /// Class representing a notification recipient.
    /// </summary>
    public class NotificationRecipient
    {
        /// <summary>
        /// Gets or sets the UKPRN.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string EmailAddress { get; set; }
    }
}