using Pds.DocumentExchange.Data.Services.DTOs;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of downloading a document.
    /// </summary>
    public interface IDocumentDownloader
    {
        /// <summary>
        /// Downloads a document as a byte array.
        /// If multiple documents are requested, returns a zip containing the documents as a byte array.
        /// </summary>
        /// <param name="exchangeDocumentDownloadRequest">The exchange document download request.</param>
        /// <param name="zipSingleDocument">A value indicating whether or not single documents should be compressed into a zip file.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<byte[]> DownloadExchangeDocument(
            ExchangeDocumentDownloadRequest exchangeDocumentDownloadRequest,
            bool zipSingleDocument = false);

        /// <summary>
        /// Downloads a file for a particular agency team.
        /// </summary>
        /// <param name="team">The name of the agency team.</param>
        /// <param name="fileName">The name of file to be downloaded.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<byte[]> DownloadAgencyDocument(string team, string fileName);
    }
}