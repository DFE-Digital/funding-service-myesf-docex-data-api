namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The Agency service configuration elements.
    /// </summary>
    public class AgencyServiceConfiguration
    {
        /// <summary>
        /// Gets or sets the service base URL.
        /// </summary>
        public string ServiceBaseURL { get; set; }
           = "https://skillsfunding.service.gov.uk/";
    }
}