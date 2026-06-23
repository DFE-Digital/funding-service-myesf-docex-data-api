using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods to retrieve the document exchange summaries.
    /// </summary>
    public interface IDocumentExchangeSummaries
    {
        /// <summary>
        /// Gets the summary information for the specified organisation user.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <returns>The summary information of the user.</returns>
        Task<Summary> GetOrganisationUserSummary(UserInfo userInfo);

        /// <summary>
        /// Gets the summary information for the specified list of agency teams.
        /// </summary>
        /// <param name="teams">The teams.</param>
        /// <returns>The summary information of the teams.</returns>
        Task<Summary> GetTeamsSummary(IEnumerable<string> teams);
    }
}