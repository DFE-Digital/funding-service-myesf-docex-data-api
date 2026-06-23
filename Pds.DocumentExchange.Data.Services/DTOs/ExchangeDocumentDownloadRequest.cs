using Pds.DocumentExchange.Data.Services.DTOs.User;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Request object representing a user's request to download one or more exchanged documents.
    /// </summary>
    public class ExchangeDocumentDownloadRequest
    {
        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// Gets or sets the list options for selecting the documents to download.
        /// </summary>
        public ExchangeListDocumentOptions ListOptions { get; set; }
    }
}