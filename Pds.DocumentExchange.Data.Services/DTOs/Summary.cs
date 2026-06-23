namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// The summary for document exchange.
    /// </summary>
    public class Summary
    {
        /// <summary>
        /// Gets or sets a value indicating whether document exchange is enabled.
        /// </summary>
        public bool DocumentExchangeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the count of new documents.
        /// </summary>
        public int? CountOfNewDocuments { get; set; }
    }
}