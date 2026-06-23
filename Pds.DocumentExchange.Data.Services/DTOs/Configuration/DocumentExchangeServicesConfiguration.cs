namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <summary>
    /// The document exchange services configuration.
    /// </summary>
    public class DocumentExchangeServicesConfiguration
    {
        /// <summary>
        /// Gets or sets the service bus queue manager configuration.
        /// </summary>
        public ServiceBusQueueManagerConfiguration ServiceBusQueueManager { get; set; }
            = new ServiceBusQueueManagerConfiguration();

        /// <summary>
        /// Gets or sets the directories manager configuration.
        /// </summary>
        public DirectoriesManagerConfiguration DirectoriesManager { get; set; }
            = new DirectoriesManagerConfiguration();

        /// <summary>
        /// Gets or sets the WindowsDefender antivirus configuration.
        /// </summary>
        public WindowsDefenderAntivirusConfiguration WindowsDefenderAntivirus { get; set; }
            = new WindowsDefenderAntivirusConfiguration();

        /// <summary>
        /// Gets or sets the virus scanner configuration.
        /// </summary>
        public VirusScannerConfiguration VirusScanner { get; set; }
            = new VirusScannerConfiguration();

        /// <summary>
        /// Gets or sets the virus scan processor configuration.
        /// </summary>
        public VirusScanProcessorConfiguration VirusScanProcessor { get; set; }
            = new VirusScanProcessorConfiguration();

        /// <summary>
        /// Gets or sets the virus scan result processor configuration.
        /// </summary>
        public VirusScanResultProcessorConfiguration VirusScanResultProcessor { get; set; }
            = new VirusScanResultProcessorConfiguration();

        /// <summary>
        /// Gets or sets the document uploader configuration.
        /// </summary>
        public DocumentUploaderConfiguration DocumentUploader { get; set; }
            = new DocumentUploaderConfiguration();

        /// <summary>
        /// Gets or sets the document publisher configuration.
        /// </summary>
        public DocumentPublisherConfiguration DocumentPublisher { get; set; }
            = new DocumentPublisherConfiguration();

        /// <summary>
        /// Gets or sets the document downloader configuration.
        /// </summary>
        public DocumentDownloaderConfiguration DocumentDownloader { get; set; }
            = new DocumentDownloaderConfiguration();

        /// <summary>
        /// Gets or sets the cosmos database encryption configuration.
        /// </summary>
        public CosmosDbConfiguration CosmosDb { get; set; }
            = new CosmosDbConfiguration();

        /// <summary>
        /// Gets or sets the agency service configuration.
        /// </summary>
        public AgencyServiceConfiguration AgencyService { get; set; }
            = new AgencyServiceConfiguration();

        /// <summary>
        /// Gets or sets the email notification configuration.
        /// </summary>
        public NotificationConfiguration Notification { get; set; }
            = new NotificationConfiguration();
    }
}