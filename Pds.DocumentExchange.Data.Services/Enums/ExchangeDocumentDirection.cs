namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Enumeration representing the direction of exchanged documents.
    /// </summary>
    public enum ExchangeDocumentDirection
    {
        /// <summary>
        /// Documents that were sent from an organisation to an agency team.
        /// </summary>
        SentByOrganisation = 0,

        /// <summary>
        /// Documents that were published by an agency team for an organisation.
        /// </summary>
        PublishedByAgency = 1
    }
}