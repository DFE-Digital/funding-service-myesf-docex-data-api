namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Message body template names.
    /// </summary>
    public enum MessageBodyTemplateName
    {
        /// <summary>
        /// ESFA published single.
        /// </summary>
        ESFAPublishedSingle,

        /// <summary>
        /// ESFA published multiple.
        /// </summary>
        ESFAPublishedMultiple,

        /// <summary>
        /// ESFA publication infected.
        /// </summary>
        ESFAPublicationInfected,

        /// <summary>
        /// ESFA single received.
        /// </summary>
        ESFAReceivedSingle,

        /// <summary>
        /// ESFA multiple received.
        /// </summary>
        ESFAReceivedMultiple,

        /// <summary>
        /// External upload completed.
        /// </summary>
        ExternalUploadCompleted,

        /// <summary>
        /// External upload infected.
        /// </summary>
        ExternalUploadInfected,

        /// <summary>
        /// Provider received multiple (document) types.
        /// </summary>
        ProviderReceivedMultipleTypes,

        /// <summary>
        /// Provider received single (document) type.
        /// </summary>
        ProviderReceivedSingleType,

        /// <summary>
        /// Provider received two (document) types.
        /// </summary>
        ProviderReceivedTwoTypes,
    }
}