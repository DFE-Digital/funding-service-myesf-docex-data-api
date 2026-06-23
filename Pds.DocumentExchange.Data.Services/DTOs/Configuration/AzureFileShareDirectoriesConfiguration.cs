namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The Azure File Share configuration.
    /// </summary>
    public class AzureFileShareDirectoriesConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the directory name.
        /// </summary>
        public string FileShareDirectoryPairs { get; set; }
    }
}