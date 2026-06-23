using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// The batch metadata analysis (implementation).
    /// </summary>
    internal sealed class BatchMetadataAnalysis :
        IBatchMetadataAnalysis
    {
        /// <inheritdoc/>
        public string ParentBatchID { get; set; }

        /// <inheritdoc/>
        public string IssuingEmail { get; set; }

        /// <inheritdoc/>
        public string IssuingPerson { get; set; }

        /// <inheritdoc/>
        public int IssuingOrganisation { get; set; }

        /// <inheritdoc/>
        public bool IsInternal { get; set; }

        /// <inheritdoc/>
        public DateTime InitialBatchDate { get; set;  }

        /// <inheritdoc/>
        public IReadOnlyCollection<int> RecipientOrganisations { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IBatchAnalysisTeam> AgencyTeams { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<BatchMetadata> Batches { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<FileMetadata> ClearFiles { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<FileMetadata> InfectedFiles { get; set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<FileMetadata> GetAllFiles() =>
            Batches.SelectMany(batch => batch.Files).AsSafeReadOnlyList();

        /// <inheritdoc/>
        public string GetProductNameFor(string productID) =>
            GetProductFor(productID)?.Name ?? "Unknown";

        /// <inheritdoc/>
        public IBatchAnalysisProduct GetProductFor(string productID)
        {
            var products = AgencyTeams.SelectMany(team => team.Products);

            return products.FirstOrDefault(product => MatchesIdentifier(product, productID));
        }

        /// <summary>
        /// Matches identifier.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns>True, if they match.</returns>
        internal bool MatchesIdentifier(IBatchAnalysisProduct product, string identifier) =>
            identifier.ComparesWith(product.Identifier);
    }
}