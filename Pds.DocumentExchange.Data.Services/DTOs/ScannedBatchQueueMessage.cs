using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// A service bus queue message with information about a batch which has been fully scanned by the antivirus.
    /// </summary>
    public class ScannedBatchQueueMessage
    {
        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        public string BatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the parent batch identifier.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the direction of the exchanged document.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeDocumentDirection DocumentDirection { get; set; }
    }
}