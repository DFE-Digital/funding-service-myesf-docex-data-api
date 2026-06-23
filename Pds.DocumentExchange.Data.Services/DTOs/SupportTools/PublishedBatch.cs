using System;

namespace Pds.DocumentExchange.Data.Services.DTOs.SupportTools
{
    /// <summary>
    /// Class representing a published batch of documents.
    /// </summary>
    public class PublishedBatch
    {
        /// <summary>
        /// Gets or sets the parent batch identifier.
        /// </summary>
        public Guid ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the user who uploaded the document.
        /// </summary>
        public FileMetadataUser UploadedBy { get; set; }

        /// <summary>
        /// Gets or sets the number of documents.
        /// </summary>
        public int NumberOfDocuments { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was uploaded.
        /// </summary>
        public DateTime DateAndTime { get; set; }

        /// <summary>
        /// Gets or sets the number of emails.
        /// </summary>
        public int NumberOfEmails { get; set; }
    }
}