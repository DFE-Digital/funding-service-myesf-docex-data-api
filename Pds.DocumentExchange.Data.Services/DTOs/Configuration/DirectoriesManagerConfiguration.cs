namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The directories manager configuration.
    /// </summary>
    public class DirectoriesManagerConfiguration
    {
        /// <summary>
        /// Gets or sets the file share directories configuration.
        /// </summary>
        public AzureFileShareDirectoriesConfiguration FileShareDirectories { get; set; }
            = new AzureFileShareDirectoriesConfiguration
            {
                ConnectionString = string.Empty,
                FileShareDirectoryPairs = @"unscanned,
                                            unscanned-processing,
                                            unscanned-processed"
            };

        /// <summary>
        /// Gets or sets the blob containers configuration.
        /// </summary>
        public AzureBlobContainerConfiguration BlobContainers { get; set; }
            = new AzureBlobContainerConfiguration
            {
                ConnectionString = string.Empty,
                Containers = "all"
            };

        /// <summary>
        /// Gets or sets the teams file share connection string.
        /// </summary>
        public string TeamsFileShareConnectionString { get; set; }
    }
}