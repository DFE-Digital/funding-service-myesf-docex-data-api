namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// An object containing the user interaction details.
    /// </summary>
    public class ClickDetail
    {
        /// <summary>
        /// Gets or sets the UKPRN the file was going to.
        /// </summary>
        public int ToUkprn { get; set; }

        /// <summary>
        /// Gets or sets the UKPRN the file was coming from.
        /// </summary>
        public int FromUkprn { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public FileMetadataUser User { get; set; }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the filename the file was uploaded with.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the click.
        /// </summary>
        public string ClickDateTimeUtc { get; set; }
    }
}