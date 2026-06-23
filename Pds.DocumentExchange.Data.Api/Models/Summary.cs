namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>The Summary for document exchange.</summary>
    public class Summary
    {
        /// <summary>Gets or sets a value indicating whether document exchange is enabled.</summary>
        public bool DocumentExchangeEnabled { get; set; }

        /// <summary>Gets or sets the count of new documents.</summary>
        public int? CountOfNewDocuments { get; set; }
    }
}