using Pds.DocumentExchange.Data.Repository.DTOs;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Metadata about a file batch.
    /// </summary>
    public class BatchMetadata : CosmosDbDocument
    {
        /// <summary>
        /// Gets or sets metadata about files contained in this batch.
        /// </summary>
        public IEnumerable<FileMetadata> Files { get; set; }

        /// <summary>
        /// Gets or sets the user who uploaded the document (external) / or who approved the publish (internal).
        /// </summary>
        public FileMetadataUser UploadedBy { get; set; }

        /// <summary>
        /// Gets or sets when the document was uploaded.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets errors during pre publish.
        /// </summary>
        public IEnumerable<string> InitialErrors { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent batch of this batch.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets an exclusive lock id for the process sending emails.
        /// </summary>
        public string EmailLockId { get; set; }
    }
}