using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>MI Report.</summary>
    public class MIReport
    {
        /// <summary>
        /// Gets or sets the ToUKPRN.
        /// </summary>
        public int ToUKPRN { get; set; }

        /// <summary>
        /// Gets or sets the FromUKPRN.
        /// </summary>
        public int FromUKPRN { get; set; }

        /// <summary>
        /// Gets or sets the provider name.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the provider type.
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// Gets or sets the document type code.
        /// </summary>
        public int FileType { get; set; }

        /// <summary>
        /// Gets or sets the document type name.
        /// </summary>
        public string FileTypeName { get; set; }

        /// <summary>
        /// Gets or sets Year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets Version.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document is from agency.
        /// </summary>
        public bool IsFromAgency { get; set; }

        /// <summary>
        /// Gets or sets the history of the file.
        /// </summary>
        public IEnumerable<FileMetadataHistory> History { get; set; }
    }
}
