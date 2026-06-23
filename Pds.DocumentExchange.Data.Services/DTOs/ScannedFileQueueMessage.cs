using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// A service bus queue message with information about a file which has been scanned by the antivirus.
    /// </summary>
    public class ScannedFileQueueMessage
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
        /// Gets or sets the scanned file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the direction of the exchanged document.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeDocumentDirection DocumentDirection { get; set; }
    }
}