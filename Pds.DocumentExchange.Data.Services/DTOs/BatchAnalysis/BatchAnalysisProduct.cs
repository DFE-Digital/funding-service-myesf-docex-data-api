using Pds.Services.Common.Helpers;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// The batch analysis product (implementation).
    /// </summary>
    internal sealed class BatchAnalysisProduct : Product,
        IBatchAnalysisProduct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisProduct"/> class.
        /// </summary>
        /// <param name="product">the product.</param>
        public BatchAnalysisProduct(Product product)
        {
            Identifier = product.Identifier;
            Name = product.Name;
            PluralName = product.PluralName;
            AgencyTeams = product.AgencyTeams;
        }

        /// <inheritdoc/>
        public int Count { get; set; }

        /// <inheritdoc/>
        IReadOnlyCollection<string> IBatchAnalysisProduct.ManagingTeams => AgencyTeams.AsSafeReadOnlyList();
    }
}