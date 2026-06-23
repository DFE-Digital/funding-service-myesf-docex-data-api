using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Factories
{
    /// <inheritdoc cref="IBatchMetadataAnalysisFactory"/>
    public sealed class BatchMetadataAnalysisFactory :
        IBatchMetadataAnalysisFactory
    {
        /// <summary>
        /// Gets the document store.
        /// </summary>
        internal ICosmosDbService DocumentStore { get; }

        /// <summary>
        /// Gets the configuration store.
        /// </summary>
        internal IConfigurationDataService ConfigurationStore { get; }

        /// <summary>
        /// Gets the encryption (service).
        /// </summary>
        internal IEncryptionService Encryption { get; }

        /// <summary>
        /// Gets the encryption key.
        /// </summary>
        internal string EncryptionKey { get; }

        private readonly ILoggerAdapter<BatchMetadataAnalysisFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchMetadataAnalysisFactory"/> class.
        /// </summary>
        /// <param name="documentStore">the document store.</param>
        /// <param name="encryption">the encrypter.</param>
        /// <param name="configurationStore">the configuration store.</param>
        /// <param name="cosmosDbConfiguration">The cosmos database encryption configuration.</param>
        /// <param name="logger">The logger.</param>
        public BatchMetadataAnalysisFactory(
            ICosmosDbService documentStore,
            IEncryptionService encryption,
            IConfigurationDataService configurationStore,
            CosmosDbConfiguration cosmosDbConfiguration,
            ILoggerAdapter<BatchMetadataAnalysisFactory> logger)
        {
            It.IsNull(documentStore)
                .AsGuard<ArgumentNullException>();
            It.IsNull(encryption)
                .AsGuard<ArgumentNullException>();
            It.IsNull(configurationStore)
                .AsGuard<ArgumentNullException>();
            It.IsNull(cosmosDbConfiguration)
                .AsGuard<ArgumentNullException>();

            DocumentStore = documentStore;
            Encryption = encryption;
            ConfigurationStore = configurationStore;
            EncryptionKey = cosmosDbConfiguration.DataEncryptionKey;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IBatchMetadataAnalysis> AnalyseBatch(string parentId)
        {
            var batches = await DocumentStore
                .GetBatchesByParent(parentId)
                .AsSafeReadOnlyList();

            _logger.LogInformation($"Started Create BatchMetadataAnalysis for parentBatchId: {parentId} with {batches?.Count} documents");

            return await Create(batches, parentId);
        }

        /// <inheritdoc/>
        public async Task<IBatchMetadataAnalysis> Create(IReadOnlyCollection<BatchMetadata> batches, string parentId)
        {
            try
            {
                var clearFiles = Collection.Empty<FileMetadata>();
                var infectedFiles = Collection.Empty<FileMetadata>();
                var recipientOrgs = Collection.Empty<int>();
                var issuingEmail = string.Empty;
                var issuingPerson = string.Empty;
                var issuingOrganisation = 0;
                var internalIssuer = false;
                var initialDate = DateTime.UtcNow.Date;

                var relevantTeams = await GetProductTeamsFor(batches);

                if (batches.Any())
                {
                    var firstBatch = batches.First();
                    var uploader = firstBatch.UploadedBy;
                    It.IsNull(uploader)
                        .AsGuard<BatchAnalysisNoUploaderException>(firstBatch.Id);

                    issuingEmail = firstBatch.UploadedBy.IsEncrypted ? Encryption.Decrypt(uploader.EmailAddress, EncryptionKey) : uploader.EmailAddress;
                    issuingPerson = firstBatch.UploadedBy.IsEncrypted ? Encryption.Decrypt(uploader.FullName, EncryptionKey) : uploader.FullName;

                    initialDate = firstBatch.CreatedDate;

                    batches.ForEach(batch =>
                    {
                        var files = batch.Files.Where(IsProcessed);
                        It.IsEmpty(files)
                            .AsGuard<BatchAnalysisBatchEmptyException>(batch.Id);

                        var firstFile = files.First();

                        if (issuingOrganisation == 0)
                        {
                            issuingOrganisation = firstFile.FromUkprn;
                            internalIssuer = firstFile.IsFromAgency;

                            // Determine if we have a mismatched senders..
                            batches
                                .SelectMany(BatchFiles)
                                .SafeAny(file => issuingOrganisation != file.FromUkprn)
                                .AsGuard<BatchAnalysisOrgCardinalityMismatchException>(batch.ParentBatchIdentifier);
                        }

                        files
                            .Where(IsClear)
                            .ForEach(clearFiles.Add);

                        files
                            .Where(file => !IsClear(file) && HasBadScanHistory(file))
                            .ForEach(infectedFiles.Add);

                        clearFiles
                            .Select(ReceivingOrganisationUkprn)
                            .ForEach(recipientOrgs.Add);
                    });
                }

                _logger.LogInformation($"Finished Create BatchMetadataAnalysis for parentBatchId: {parentId} with {batches?.Count} documents");

                return new BatchMetadataAnalysis
                {
                    ParentBatchID = parentId,
                    IssuingEmail = issuingEmail,
                    IssuingPerson = issuingPerson,
                    IssuingOrganisation = issuingOrganisation,
                    IsInternal = internalIssuer,
                    RecipientOrganisations = recipientOrgs.Distinct().AsSafeReadOnlyList(),
                    AgencyTeams = relevantTeams.AsSafeReadOnlyList(),
                    Batches = batches,
                    InitialBatchDate = initialDate,
                    ClearFiles = clearFiles.AsSafeReadOnlyList(),
                    InfectedFiles = infectedFiles.AsSafeReadOnlyList(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Create)} for parentBatchId: {parentId}");
                throw;
            }
        }

        /// <summary>
        /// Batch files.
        /// </summary>
        /// <param name="batch">The batch.</param>
        /// <returns>The files.</returns>
        internal IEnumerable<FileMetadata> BatchFiles(BatchMetadata batch) =>
            batch.Files;

        /// <summary>
        /// Target organisation id.
        /// </summary>
        /// <param name="metadata">The file.</param>
        /// <returns>The provider id.</returns>
        internal int ReceivingOrganisationUkprn(FileMetadata metadata) =>
            metadata.ToUkprn;

        /// <summary>
        /// Is processed.
        /// </summary>
        /// <param name="metadata">The file.</param>
        /// <returns>True, if the file is processed.</returns>
        internal bool IsProcessed(FileMetadata metadata) =>
            metadata.Processed;

        /// <summary>
        /// Is clear (of virus's).
        /// </summary>
        /// <param name="metadata">The file.</param>
        /// <returns>True, if the file is clear.</returns>
        internal bool IsClear(FileMetadata metadata) =>
            metadata.VirusScanSuccessful;

        /// <summary>
        /// Has bad scan history.
        /// </summary>
        /// <param name="metadata">The file.</param>
        /// <returns>True, if the file has a bad scan history.</returns>
        internal bool HasBadScanHistory(FileMetadata metadata) =>
            metadata.History.SafeAny(history => history.Action == FileAction.FileScanBad);

        /// <summary>
        /// Is product.
        /// </summary>
        /// <param name="metadata">The file.</param>
        /// <param name="product">The product.</param>
        /// <returns>True, if the product matches the file.</returns>
        internal bool IsProduct(FileMetadata metadata, BatchAnalysisProduct product) =>
            metadata.ProductIdentifier.ComparesWith(product.Identifier);

        /// <summary>
        /// Contain products.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="products">The products.</param>
        /// <returns>True, if the products are in the files.</returns>
        internal bool ContainProducts(IEnumerable<FileMetadata> files, IEnumerable<BatchAnalysisProduct> products) =>
            products.SafeAny(product => files.SafeAny(file => IsProduct(file, product)));

        /// <summary>
        /// Get the product teams for...
        /// </summary>
        /// <param name="batches">The batches.</param>
        /// <returns>A collection of teams, along with the products assigned to them.</returns>
        internal async Task<IReadOnlyCollection<IBatchAnalysisTeam>> GetProductTeamsFor(IReadOnlyCollection<BatchMetadata> batches)
        {
            var allTeams = (await ConfigurationStore.GetTeams())
                .Select(team => new BatchAnalysisTeam(team))
                .AsSafeReadOnlyList();

            var allProducts = await ConfigurationStore.GetProducts();

            allTeams.ForEach(team =>
            {
                allProducts.ForEach(product =>
                {
                    if (product.AgencyTeams.Contains(team.Identifier))
                    {
                        team.Products.Add(new BatchAnalysisProduct(product));
                    }
                });
            });

            var files = batches
                .SelectMany(BatchFiles)
                .AsSafeReadOnlyList();
            var relevantTeams = allTeams
                .Where(team => ContainProducts(files, team.Products))
                .AsSafeReadOnlyList();

            relevantTeams.ForEach(team =>
            {
                team.Products.ForEach(product =>
                {
                    product.Count = files.Count(file => IsProduct(file, product));
                });
            });

            return relevantTeams;
        }
    }
}