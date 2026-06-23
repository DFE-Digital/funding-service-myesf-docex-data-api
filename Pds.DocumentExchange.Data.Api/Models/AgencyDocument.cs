namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Class representing a document in an agency file share.
    /// </summary>
    public class AgencyDocument : Document
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the file name error (if applicable).
        /// </summary>
        public string FileNameError { get; set; }

        /// <summary>
        /// Gets or sets the team the document belongs to.
        /// </summary>
        public string Team { get; set; }
    }
}