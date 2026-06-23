using Newtonsoft.Json;
using Pds.DocumentExchange.Data.Repository.Common;

namespace Pds.DocumentExchange.Data.Repository.DTOs
{
    /// <summary>
    /// The Cosmos Db document abstract class.
    /// </summary>
    public abstract class CosmosDbDocument
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = CosmosDbConstants.DefaultPartitionKey;

        /// <summary>
        /// Gets or sets the unique identifier for the batch.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}