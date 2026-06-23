using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// A representation of actions provided by the Agency service.
    /// </summary>
    public interface IAgencyService
    {
        /// <summary>
        /// Gets a summary of the agency files contained within the file share for the given teams.
        /// </summary>
        /// <param name="teams">The agency teams for which file share summary has to be retrieved.</param>
        /// <returns>The action result containing, if successful,
        /// a summary of the agency teams' files.</returns>
        Task<FileShareSummary> Summary(IEnumerable<string> teams);

        /// <summary>
        /// Gets the agency files contained within the file share for the given teams based on the specified options.
        /// </summary>
        /// <param name="teams">The agency teams for which the files have to be retrieved.</param>
        /// <param name="options">The agency list document options.</param>
        /// <returns>A list result containing the filtered and paged documents
        /// along with the updated filters.</returns>
        Task<ListResult<AgencyDocument>> GetDocuments(IEnumerable<string> teams, AgencyListDocumentOptions options);
    }
}