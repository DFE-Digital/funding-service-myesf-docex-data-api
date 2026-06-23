using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Information about a file which has been scanned by the antivirus.
    /// </summary>
    public class ScannedFileInfo
    {
        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        public string BatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the scanned file info.
        /// </summary>
        public FileMetadata FileInfo { get; set; }

        /// <summary>
        /// Gets or sets the direction of the exchanged document.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeDocumentDirection DocumentDirection { get; set; }

        /// <summary>
        /// Gets or sets the virus scan result of the file.
        /// </summary>
        public FileSafety ScanResult { get; set; }
    }
}