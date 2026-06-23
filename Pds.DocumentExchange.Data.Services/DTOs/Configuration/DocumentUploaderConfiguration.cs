namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The document uploader configuration.
    /// </summary>
    public class DocumentUploaderConfiguration
    {
        /// <summary>
        /// Gets or sets the directory key where documents are uploaded.
        /// </summary>
        public string UploadDirectoryKey { get; set; } = "unscanned";

        /// <summary>
        /// Gets or sets the virus scan required queue name.
        /// </summary>
        public string VirusScanRequiredQueueName { get; set; } = "virusscanrequired";
    }
}