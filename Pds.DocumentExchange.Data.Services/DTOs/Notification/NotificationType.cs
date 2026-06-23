namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// Enumeration of notification types.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Not set.
        /// </summary>
        NotSet,

        /// <summary>
        /// Provider uploaded an infected document.
        /// </summary>
        ProviderUploadInfected,

        /// <summary>
        /// Provider uploaded a clear document.
        /// </summary>
        ProviderUploadClear,

        /// <summary>
        /// Provider upload was received by the agency.
        /// </summary>
        ProviderUploadReceived,

        /// <summary>
        /// Agency published an infected document.
        /// </summary>
        AgencyPublishInfected,

        /// <summary>
        /// Agency published a clear document.
        /// </summary>
        AgencyPublishClear,
    }
}