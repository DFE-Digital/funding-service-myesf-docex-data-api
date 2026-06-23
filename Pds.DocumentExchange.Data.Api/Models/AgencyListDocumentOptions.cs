using Pds.DocumentExchange.Data.Api.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Contains options for selecting agency documents.
    /// </summary>
    public class AgencyListDocumentOptions : ListOptions
    {
        /// <summary>
        /// Gets or sets the validity option, for selecting the documents by their validity.
        /// </summary>
        public AgencyDocumentValidity Validity { get; set; }

        /// <summary>
        /// Gets or sets the document names to include.
        /// If not specified, documents will not be selected by name.
        /// </summary>
        public IEnumerable<string> DocumentNames { get; set; }
    }
}