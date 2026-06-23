using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// A service bus queue message representing a virus scan request for a (parent) batch.
    /// </summary>
    public class VirusScanRequestMessage
    {
        /// <summary>
        /// Gets or sets the (parent) batch identifier.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the direction of the exchanged documents.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeDocumentDirection DocumentDirection { get; set; }
    }
}