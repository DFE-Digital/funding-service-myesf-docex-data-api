namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// Configuration for the email notification processor.
    /// </summary>
    public class NotificationConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use the DfE Sign-in Contacts API to lookup provider emails.
        /// </summary>
        /// <remarks>If false, provider emails are looked up using IDAMS.</remarks>
        public bool UseDfeSignInContactsApi { get; set; } = false;
    }
}