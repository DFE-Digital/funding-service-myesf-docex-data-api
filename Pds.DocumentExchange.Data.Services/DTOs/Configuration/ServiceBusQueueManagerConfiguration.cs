namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The service bus queue manager configuration.
    /// </summary>
    public class ServiceBusQueueManagerConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the queue names.
        /// </summary>
        public string QueueNames { get; set; }
            = @"virusscanrequired,
                virusscangood,
                virusscanbad,
                readyforemail";
    }
}