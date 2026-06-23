namespace Pds.DocumentExchange.Data.Repository.DTOs.Configuration
{
    /// <summary>
    /// The Azure CosmosDb repository configuration.
    /// </summary>
    public class AzureCosmosDbRepositoryConfiguration
    {
        /// <summary>
        /// Gets or sets the service endpoint.
        /// </summary>
        public string ServiceEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the authorisation key or resource token.
        /// </summary>
        public string AuthKeyOrResourceToken { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName { get; set; } = "docx";

        /// <summary>
        /// Gets or sets the collection name.
        /// </summary>
        public string CollectionName { get; set; } = "collection1";

        /// <summary>
        /// Gets or sets the policy for the maximum number of retries
        /// to attempt when Cosmos DB throttles requests.
        /// </summary>
        public int MaxRetryAttemptsOnThrottledRequests { get; set; } = 10;

        /// <summary>
        /// Gets or sets the policy for the maximum cumulative retry wait time
        /// to use when Cosmos DB throttles requests.
        /// </summary>
        public int MaxRetryWaitTimeInSeconds { get; set; } = 60;
    }
}