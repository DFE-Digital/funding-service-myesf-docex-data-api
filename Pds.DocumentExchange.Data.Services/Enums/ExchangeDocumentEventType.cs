namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Enumeration of the events that can be logged against an exchange document.
    /// </summary>
    public enum ExchangeDocumentEventType
    {
        /// <summary>
        /// Downloaded by the sender.
        /// </summary>
        DownloadedBySender = 1,

        /// <summary>
        /// Downloaded by the receiver.
        /// </summary>
        DownloadedByReceiver = 2,

        /// <summary>
        /// Sent by an organisation user.
        /// </summary>
        SentByOrganisation = 6,

        /// <summary>
        /// Published by an agency team user.
        /// </summary>
        PublishedByAgency = 8
    }
}