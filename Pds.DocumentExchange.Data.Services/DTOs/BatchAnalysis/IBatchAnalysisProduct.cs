using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// I batch analysis product (contract).
    /// </summary>
    public interface IBatchAnalysisProduct
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        int Identifier { get; }

        /// <summary>
        /// Gets product name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets product plural name.
        /// </summary>
        string PluralName { get; }

        /// <summary>
        /// Gets the product count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the managing agency teams.
        /// </summary>
        IReadOnlyCollection<string> ManagingTeams { get; }
    }
}