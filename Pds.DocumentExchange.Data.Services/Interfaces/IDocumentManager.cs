using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of managing documents.
    /// </summary>
    public interface IDocumentManager
    {
        /// <summary>
        /// Removes a list of files for a particular agency team.
        /// </summary>
        /// <param name="team">The name of the agency team.</param>
        /// <param name="fileNames">The name of files to be removed.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task RemoveAgencyDocuments(string team, IEnumerable<string> fileNames);
    }
}