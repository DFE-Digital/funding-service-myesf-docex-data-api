using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// The reference information of a document.
    /// </summary>
    public class DocumentReferenceWithPreviousVersions : DocumentReference
    {
        /// <summary>
        /// Gets or sets the collection of document references for previous versions of this document.
        /// </summary>
        public IEnumerable<DocumentReference> PreviousVersions { get; set; } = Enumerable.Empty<DocumentReference>();
    }
}