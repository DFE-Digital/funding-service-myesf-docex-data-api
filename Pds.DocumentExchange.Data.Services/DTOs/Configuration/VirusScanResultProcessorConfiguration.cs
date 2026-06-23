namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The virus scan result processor configuration.
    /// </summary>
    public class VirusScanResultProcessorConfiguration
    {
        /// <summary>
        /// Gets or sets the agency's source directory key.
        /// </summary>
        public string AgencySourceDirectoryKey { get; set; } = "unscanned-processing";

        /// <summary>
        /// Gets or sets the agency's destination directory key.
        /// </summary>
        public string AgencyDestinationDirectoryKey { get; set; } = "unscanned-processed";

        /// <summary>
        /// Gets or sets the organisation's source directory key.
        /// </summary>
        public string OrganisationSourceDirectoryKey { get; set; } = "unscanned";

        /// <summary>
        /// Gets or sets the organisation's destination directory key.
        /// </summary>
        public string OrganisationDestinationDirectoryKey { get; set; } = "unscanned-processed";

        /// <summary>
        /// Gets or sets the file copy destination directory key.
        /// </summary>
        public string FileCopyDestinationDirectoryKey { get; set; } = "all";

        /// <summary>
        /// Gets or sets the ready for email queue name.
        /// </summary>
        public string ReadyForEmailQueue { get; set; } = "readyforemail";
    }
}