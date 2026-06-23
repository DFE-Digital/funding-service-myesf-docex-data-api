using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// I batch analysis (agency) team (contract).
    /// </summary>
    public interface IBatchAnalysisTeam
    {
        /// <summary>
        /// Gets the team name, e.g. 'DocumentExchangeAdministratorFundingCentre'.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the team's email address.
        /// </summary>
        string EmailAddress { get; }

        /// <summary>
        /// Gets the agency teams' products.
        /// </summary>
        IReadOnlyCollection<IBatchAnalysisProduct> Products { get; }
    }
}