using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Request object representing a user's request to delete one or more exchanged documents.
    /// </summary>
    public class ExchangeDocumentDeleteRequest
    {
        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// Gets or sets the list of references to the documents to delete.
        /// </summary>
        public IEnumerable<DocumentReferenceWithPreviousVersions> DocumentReferences { get; set; }
    }
}