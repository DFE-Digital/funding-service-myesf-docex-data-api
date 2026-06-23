using Pds.DocumentExchange.Data.Services.DTOs;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide options for processing virus scan results.
    /// </summary>
    public interface IVirusScanResultProcessor
    {
        /// <summary>
        /// Processes a successful virus scan of an agency's file.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The file metadata info.</returns>
        Task<FileMetadata> ProcessAgencyFileResultOk(DocumentReference documentReference);

        /// <summary>
        /// Processes an unsuccessful virus scan of an agency's file.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The file metadata info.</returns>
        Task<FileMetadata> ProcessAgencyFileResultVirusFound(DocumentReference documentReference);

        /// <summary>
        /// Processes a successful virus scan of an organisation's file.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The file metadata info.</returns>
        Task<FileMetadata> ProcessOrganisationFileResultOk(DocumentReference documentReference);

        /// <summary>
        /// Processes an unsuccessful virus scan of an organisation's file.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The file metadata info.</returns>
        Task<FileMetadata> ProcessOrganisationFileResultVirusFound(DocumentReference documentReference);
    }
}