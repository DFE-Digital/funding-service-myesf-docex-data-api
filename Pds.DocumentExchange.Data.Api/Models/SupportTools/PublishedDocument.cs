namespace Pds.DocumentExchange.Data.Api.Models.SupportTools
{
    /// <summary>
    /// Class representing a published document.
    /// </summary>
    public class PublishedDocument
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file type.
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets or sets which provider should receive the document.
        /// </summary>
        public int ToUKPRN { get; set; }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the virus scan verified the file as virus-free.
        /// </summary>
        public bool VirusScanSuccessful { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification email was prepared to be sent.
        /// </summary>
        public bool EmailPrepared { get; set; }
    }
}