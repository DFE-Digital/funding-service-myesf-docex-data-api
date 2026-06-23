namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The virus scanner configuration.
    /// </summary>
    public class VirusScannerConfiguration
    {
        /// <summary>
        /// Gets or sets the agency's directory key.
        /// </summary>
        public string AgencyDirectoryKey { get; set; } = "unscanned-processing";

        /// <summary>
        /// Gets or sets the organisation's directory key.
        /// </summary>
        public string OrganisationDirectoryKey { get; set; } = "unscanned";
    }
}