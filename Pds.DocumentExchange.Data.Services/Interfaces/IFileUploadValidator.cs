using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Provides the methods for validating an uploaded file.
    /// </summary>
    public interface IFileUploadValidator
    {
        /// <summary>
        /// Validates the uploaded files.
        /// </summary>
        /// <param name="request">A HttpRequest containing the uploaded files.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ValidateUpload(HttpRequest request);
    }
}
