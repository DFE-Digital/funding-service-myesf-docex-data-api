namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The virus scan processor configuration.
    /// </summary>
    public class VirusScanProcessorConfiguration
    {
        /// <summary>
        /// Gets or sets the max. batch size.
        /// </summary>
        public int MaxBatchSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the successful scan queue name.
        /// </summary>
        public string SuccessfulScanQueueName { get; set; } = "virusscangood";

        /// <summary>
        /// Gets or sets the virus found queue name.
        /// </summary>
        public string VirusFoundScanQueueName { get; set; } = "virusscanbad";
    }
}