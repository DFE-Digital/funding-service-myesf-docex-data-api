namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>The click summary.</summary>
    public class ClickSummary
    {
        /// <summary>
        /// Gets or sets the UKPRN the file was going to.
        /// </summary>
        public int ToUkprn { get; set; }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the filename the file was uploaded with.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets how many clicks it was.
        /// </summary>
        public int ClickCount { get; set; }
    }
}