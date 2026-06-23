namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The document downloader configuration.
    /// </summary>
    public class DocumentDownloaderConfiguration
    {
        /// <summary>
        /// Gets or sets the directory key where documents are downloaded from.
        /// </summary>
        public string DownloadDirectoryKey { get; set; } = "all";

        /// <summary>
        /// Gets or sets in how many hours from now the file expires.
        /// </summary>
        public double ExpiresInHours { get; set; } = 7 * 24;
    }
}