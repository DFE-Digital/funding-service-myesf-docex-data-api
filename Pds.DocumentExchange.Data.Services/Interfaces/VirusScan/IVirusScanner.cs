using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of scanning a file and get information back about the process.
    /// </summary>
    public interface IVirusScanner
    {
        /// <summary>
        /// Runs a virus scan on a file and gets the scan result back with the updated file info.
        /// </summary>
        /// <param name="fileInfo">The file metadata.</param>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <param name="documentDirection">The direction of the exchanged document.</param>
        /// <returns>A ScannedFileInfo object containing information about the scan result and the
        /// updated file metadata.</returns>
        Task<ScannedFileInfo> ScanFile(
            FileMetadata fileInfo,
            string batchIdentifier,
            string parentBatchIdentifier,
            ExchangeDocumentDirection documentDirection);


        /// <summary>
        /// Runs a virus scan on a list of files and gets the scan result back with the updated file info.
        /// </summary>
        /// <param name="filesInfo">The list of files metadata.</param>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <param name="documentDirection">The direction of the exchanged document.</param>
        /// <returns>A list of ScannedFileInfo objects containing information about the scan result and the
        /// updated file metadata.</returns>
        IAsyncEnumerable<ScannedFileInfo> ScanFiles(
            IEnumerable<FileMetadata> filesInfo,
            string batchIdentifier,
            string parentBatchIdentifier,
            ExchangeDocumentDirection documentDirection);
    }
}