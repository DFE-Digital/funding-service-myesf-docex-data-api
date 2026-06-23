namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Model representing a summary of the agency documents contained within a file share.
    /// </summary>
    public class FileShareSummary
    {
        /// <summary>
        /// Gets or sets the total count of valid documents within the file share.
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// Gets or sets the total count of invalid documents within the file share.
        /// </summary>
        public int InvalidCount { get; set; }
    }
}