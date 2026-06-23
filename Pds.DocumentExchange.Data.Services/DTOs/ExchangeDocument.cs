using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Class representing a document that has been exchanged.
    /// </summary>
    public class ExchangeDocument : Document
    {
        /// <summary>
        /// Gets or sets the document reference.
        /// </summary>
        public DocumentReference DocumentReference { get; set; }

        /// <summary>
        /// Gets or sets the agency team who have sent or received this document.
        /// </summary>
        public string AgencyTeam { get; set; }

        /// <summary>
        /// Gets or sets the direction of the exchange for this document.
        /// </summary>
        public ExchangeDocumentDirection ExchangeDirection { get; set; }

        /// <summary>
        /// Gets or sets the history of events for this document.
        /// </summary>
        public IEnumerable<ExchangeDocumentEvent> EventHistory { get; set; }

        /// <summary>
        /// Gets or sets the file version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the collection of previous versions of this document.
        /// </summary>
        public IEnumerable<ExchangeDocument> PreviousVersions { get; set; }
    }
}