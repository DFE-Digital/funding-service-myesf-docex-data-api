using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Contains options for selecting exchanged documents.
    /// </summary>
    public class ExchangeListDocumentOptions : ListOptions
    {
        /// <summary>
        /// Gets or sets the document status option, for selecting the documents by their status.
        /// </summary>
        public ExchangeDocumentDirection DocumentStatusOption { get; set; }

        /// <summary>
        /// Gets or sets the document references to include.
        /// If not specified, documents will not be selected by their reference.
        /// </summary>
        public IEnumerable<DocumentReference> DocumentReferences { get; set; }
    }
}