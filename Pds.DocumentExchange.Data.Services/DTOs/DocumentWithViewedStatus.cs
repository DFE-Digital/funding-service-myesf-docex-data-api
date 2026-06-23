namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// The reference information of a document with seen status.
    /// </summary>
    public class DocumentWithViewedStatus
    {
        /// <summary>
        /// Gets or sets the file type.
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the Year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets the sender Ukprn.
        /// </summary>
        public int FromUKPRN { get; set; }

        /// <summary>
        /// Gets or sets the Version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document has been viewed by receiver or not.
        /// </summary>
        public bool Viewed { get; set; }
    }
}