using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis
{
    /// <summary>
    /// I batch metadata analysis (contract).
    /// </summary>
    public interface IBatchMetadataAnalysis
    {
        /// <summary>
        /// Gets the parent batch id.
        /// </summary>
        string ParentBatchID { get; }

        /// <summary>
        /// Gets the issuing email address.
        /// </summary>
        string IssuingEmail { get; }

        /// <summary>
        /// Gets the issuing person's name.
        /// </summary>
        string IssuingPerson { get; }

        /// <summary>
        /// Gets the issuing organisation.
        /// </summary>
        int IssuingOrganisation { get; }

        /// <summary>
        /// Gets a value indicating whether the issuing organisation is internal.
        /// </summary>
        bool IsInternal { get; }

        /// <summary>
        /// Gets the initial batch date.
        /// </summary>
        DateTime InitialBatchDate { get; }

        /// <summary>
        /// Gets the recipient organisation id's.
        /// </summary>
        IReadOnlyCollection<int> RecipientOrganisations { get; }

        /// <summary>
        /// Gets the agency teams.
        /// </summary>
        IReadOnlyCollection<IBatchAnalysisTeam> AgencyTeams { get; }

        /// <summary>
        /// Gets the analysed batches.
        /// </summary>
        IReadOnlyCollection<BatchMetadata> Batches { get; }

        /// <summary>
        /// Gets the clear files collection.
        /// </summary>
        IReadOnlyCollection<FileMetadata> ClearFiles { get; }

        /// <summary>
        /// Gets the infected files collection.
        /// </summary>
        IReadOnlyCollection<FileMetadata> InfectedFiles { get; }

        /// <summary>
        /// Gets the product name for...
        /// </summary>
        /// <param name="productID">The product identifier.</param>
        /// <returns>The product name, or 'unknown'.</returns>
        string GetProductNameFor(string productID);

        /// <summary>
        /// Gets the product for...
        /// </summary>
        /// <param name="productID">The product identifier.</param>
        /// <returns>The product or null.</returns>
        IBatchAnalysisProduct GetProductFor(string productID);

        /// <summary>
        /// Gets all files included in the batch.
        /// </summary>
        /// <returns>All the files in all the batches.</returns>
        IReadOnlyCollection<FileMetadata> GetAllFiles();
    }
}