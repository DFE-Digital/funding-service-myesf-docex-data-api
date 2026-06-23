using Pds.DocumentExchange.Data.Services.DTOs;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of uploading a document.
    /// </summary>
    public interface IDocumentUploader
    {
        /// <summary>
        /// Uploads a document.
        /// </summary>
        /// <param name="uploadDocumentRequest">The upload document request.</param>
        /// <returns>An awaitable task.</returns>
        Task UploadDocument(UploadDocumentRequest uploadDocumentRequest);
    }
}