using Pds.Services.Common.Helpers;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// The batch analysis (agency) team (implementation).
    /// </summary>
    internal sealed class BatchAnalysisTeam :
        AgencyTeam,
        IBatchAnalysisTeam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisTeam"/> class.
        /// </summary>
        /// <param name="agencyTeam">the agency team.</param>
        public BatchAnalysisTeam(AgencyTeam agencyTeam)
        {
            Identifier = agencyTeam.Identifier;
            Name = agencyTeam.Name;
            EmailAddress = agencyTeam.EmailAddress;
        }

        /// <summary>
        /// Gets or sets the 'manageable part' of the products collection.
        /// </summary>
        public ICollection<BatchAnalysisProduct> Products { get; set; } = Collection.Empty<BatchAnalysisProduct>();

        /// <inheritdoc/>
        IReadOnlyCollection<IBatchAnalysisProduct> IBatchAnalysisTeam.Products => Products.AsSafeReadOnlyList();
    }
}