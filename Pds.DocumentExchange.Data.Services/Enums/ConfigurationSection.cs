namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Enumeration of the named sections of the Document Exchange configuration data.
    /// </summary>
    public enum ConfigurationSection
    {
        /// <summary>
        /// The configuration section for the Document Exchange feature toggle value.
        /// </summary>
        DocumentExchangeEnabled,

        /// <summary>
        /// The configuration section for the Document Exchange organisation upload feature toggle value.
        /// </summary>
        DocumentExchangeOrganisationUploadEnabled,

        /// <summary>
        /// The configuration section for the Document Exchange agency teams.
        /// </summary>
        Teams,

        /// <summary>
        /// The configuration section for the Document Exchange products.
        /// </summary>
        Products,

        /// <summary>
        /// The configuration section for the allowed file extensions.
        /// </summary>
        AllowedFileExtensions,

        /// <summary>
        /// The configuration section for the service reply email.
        /// </summary>
        ServiceReplyEmail,

        /// <summary>
        /// The configuration section for the service no-reply email.
        /// </summary>
        ServiceNoReplyEmail,

        /// <summary>
        /// The agency publish complete provider notification enabled
        /// </summary>
        AgencyPublishCompleteProviderNotificationEnabled,

        /// <summary>
        /// The configuration section for the maximum file upload size in kilobytes.
        /// </summary>
        MaxFileUploadSize,

        /// <summary>
        /// The configuration section for the notify email settings.
        /// </summary>
        EmailSettings,

        /// <summary>
        /// The configuration section for the reject SPI invalid data setting for the cache warm-up function.
        /// </summary>
        CacheWarmUpRejectSpiInvalidData
    }
}