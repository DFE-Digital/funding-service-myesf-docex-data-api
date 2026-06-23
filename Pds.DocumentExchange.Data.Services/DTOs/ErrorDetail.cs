namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>An object containing the error details.</summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Gets or sets when the error was raised.
        /// </summary>
        public string CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the inital errors against an upload.
        /// </summary>
        public string InitialErrors { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public FileMetadataUser User { get; set; }
    }
}