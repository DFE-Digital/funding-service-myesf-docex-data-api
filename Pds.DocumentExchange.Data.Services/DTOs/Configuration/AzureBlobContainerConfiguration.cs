namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The Azure blob container configuration.
    /// </summary>
    public class AzureBlobContainerConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the container names.
        /// </summary>
        public string Containers { get; set; }
    }
}