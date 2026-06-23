namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The document publisher configuration.
    /// </summary>
    public class DocumentPublisherConfiguration
    {
        /// <summary>
        /// Gets or sets the agency's destination directory key.
        /// </summary>
        public string AgencyDestinationDirectoryKey { get; set; } = "unscanned-processing";

        /// <summary>
        /// Gets or sets the virus scan required queue name.
        /// </summary>
        public string VirusScanRequiredQueueName { get; set; } = "virusscanrequired";
    }
}