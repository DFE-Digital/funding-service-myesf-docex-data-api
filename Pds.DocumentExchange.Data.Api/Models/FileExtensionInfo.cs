using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Represents a file extension.
    /// </summary>
    public class FileExtensionInfo
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an internal user can upload a file with the extension.
        /// </summary>
        public bool CanInternalUserUpload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an external user can upload a file with the extension.
        /// </summary>
        public bool CanExternalUserUpload { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the products for which the file extension is valid.
        /// </summary>
        public IEnumerable<int> ProductIdentifiers { get; set; }
    }
}